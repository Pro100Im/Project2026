using UnityEngine;

namespace Code.Common.Network
{
    public interface INetworkConnectionService
    {
        public void QuickMatch();
        public void CancelSerching();
    }
}