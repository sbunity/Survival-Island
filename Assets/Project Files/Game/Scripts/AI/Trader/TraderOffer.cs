using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class TraderOffer
    {
        [SerializeField] Resource[] give;
        public Resource[] Give => give;

        [SerializeField] Resource[] receive;
        public Resource[] Receive => receive;

        public bool IsUnlocked => ResourceUnlocks.AreAllUnlocked(give) && ResourceUnlocks.AreAllUnlocked(receive);
    }
}
