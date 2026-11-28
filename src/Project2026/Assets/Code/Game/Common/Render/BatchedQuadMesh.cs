using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Code.Game.Common.Render
{
    /// <summary>
    /// A layer of axis-aligned quads drawn as one mesh, so the whole layer is a single draw call
    /// (HP bars, damage numbers). It is a MeshRenderer, not Graphics.RenderMesh, so it takes part in 2D sorting
    /// by sortingOrder like any sprite.
    /// Memory is allocated once for a fixed quad capacity (per level); filling and uploading allocate nothing.
    /// Quads past the capacity are dropped.
    /// Usage per frame: Begin → TryAdd... → End.
    /// </summary>
    public sealed class BatchedQuadMesh : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct QuadVertex
        {
            public Vector3 Position;
            public Color32 Color;
            public Vector2 Uv;
        }

        private const MeshUpdateFlags UpdateFlags =
            MeshUpdateFlags.DontValidateIndices |
            MeshUpdateFlags.DontResetBoneBounds |
            MeshUpdateFlags.DontNotifyMeshUsers |
            MeshUpdateFlags.DontRecalculateBounds;

        // Never culled: the layer covers the whole level.
        private static readonly Bounds HugeBounds = new(Vector3.zero, new Vector3(100000f, 100000f, 1000f));

        private static readonly VertexAttributeDescriptor[] Layout =
        {
            new(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
            new(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
        };

        private readonly GameObject _gameObject;
        private readonly Mesh _mesh;

        private NativeArray<QuadVertex> _vertices;
        private int _capacity;
        private int _count;
        private int _uploadedCount = -1;

        public int Capacity => _capacity;
        public int Count => _count;

        public BatchedQuadMesh(string name, Material material, int sortingOrder, int capacity)
        {
            _mesh = new Mesh { name = name, hideFlags = HideFlags.DontSave };
            _mesh.MarkDynamic();

            _gameObject = new GameObject(name);
            _gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh;

            var renderer = _gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.allowOcclusionWhenDynamic = false;

            Allocate(Mathf.Max(1, capacity));
        }

        /// <summary>Grows the buffers to <paramref name="capacity"/> quads. Only reallocates when it is larger.</summary>
        public void EnsureCapacity(int capacity)
        {
            if (capacity > _capacity)
                Allocate(capacity);
        }

        public void Begin() => _count = 0;

        /// <summary>Adds a quad from (x0, y0) to (x1, y1), uv rect (uMin, vMin, uMax, vMax). False when full.</summary>
        public bool TryAdd(float x0, float y0, float x1, float y1, Color32 color, Vector4 uv)
        {
            if (_count >= _capacity)
                return false;

            var v = _count * 4;

            _vertices[v] = new QuadVertex { Position = new Vector3(x0, y0, 0f), Color = color, Uv = new Vector2(uv.x, uv.y) };
            _vertices[v + 1] = new QuadVertex { Position = new Vector3(x0, y1, 0f), Color = color, Uv = new Vector2(uv.x, uv.w) };
            _vertices[v + 2] = new QuadVertex { Position = new Vector3(x1, y1, 0f), Color = color, Uv = new Vector2(uv.z, uv.w) };
            _vertices[v + 3] = new QuadVertex { Position = new Vector3(x1, y0, 0f), Color = color, Uv = new Vector2(uv.z, uv.y) };

            _count++;
            return true;
        }

        public void End()
        {
            if (_count == 0 && _uploadedCount == 0)
                return;

            if (_count > 0)
                _mesh.SetVertexBufferData(_vertices, 0, 0, _count * 4, 0, UpdateFlags);

            _mesh.SetSubMesh(0, new SubMeshDescriptor(0, _count * 6)
            {
                bounds = HugeBounds,
                firstVertex = 0,
                vertexCount = _count * 4,
            }, UpdateFlags);

            _uploadedCount = _count;
        }

        public void Dispose()
        {
            if (_vertices.IsCreated)
                _vertices.Dispose();

            if (_gameObject != null)
                Object.Destroy(_gameObject);

            if (_mesh != null)
                Object.Destroy(_mesh);
        }

        private void Allocate(int capacity)
        {
            if (_vertices.IsCreated)
                _vertices.Dispose();

            _capacity = capacity;
            _count = 0;
            _uploadedCount = -1;
            _vertices = new NativeArray<QuadVertex>(capacity * 4, Allocator.Persistent, NativeArrayOptions.ClearMemory);

            var vertexCount = capacity * 4;
            var indexCount = capacity * 6;
            var use32 = vertexCount > ushort.MaxValue;

            _mesh.Clear();
            _mesh.SetVertexBufferParams(vertexCount, Layout);
            _mesh.SetIndexBufferParams(indexCount, use32 ? IndexFormat.UInt32 : IndexFormat.UInt16);

            if (use32)
                FillIndices<uint>(capacity, i => (uint)i);
            else
                FillIndices<ushort>(capacity, i => (ushort)i);

            _mesh.subMeshCount = 1;
            _mesh.SetSubMesh(0, new SubMeshDescriptor(0, 0) { bounds = HugeBounds }, UpdateFlags);
            _mesh.bounds = HugeBounds;
        }

        private void FillIndices<T>(int capacity, Func<int, T> convert) where T : struct
        {
            var indices = new NativeArray<T>(capacity * 6, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            for (var q = 0; q < capacity; q++)
            {
                var v = q * 4;
                var i = q * 6;

                indices[i] = convert(v);
                indices[i + 1] = convert(v + 1);
                indices[i + 2] = convert(v + 2);
                indices[i + 3] = convert(v);
                indices[i + 4] = convert(v + 2);
                indices[i + 5] = convert(v + 3);
            }

            _mesh.SetIndexBufferData(indices, 0, 0, indices.Length, UpdateFlags);
            indices.Dispose();
        }
    }
}
