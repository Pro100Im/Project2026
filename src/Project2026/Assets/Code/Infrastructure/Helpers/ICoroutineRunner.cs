using System.Collections;
using UnityEngine;

namespace Code.Infrastructure.Helpers
{
    public interface ICoroutineRunner
    {
        Coroutine StartCoroutine(IEnumerator load);
    }
}