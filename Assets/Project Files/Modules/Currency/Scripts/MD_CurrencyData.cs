using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class CurrencyData
    {
        [SerializeField] bool displayAlways = false;
        public bool DisplayAlways => displayAlways;

        [SerializeField] GameObject flyingResPrefab;
        public GameObject FlyingResPrefab => flyingResPrefab;

        [SerializeField] GameObject dropResPrefab;
        public GameObject DropResPrefab => dropResPrefab;

        [SerializeField] AudioClip pickUpSound;
        public AudioClip PickUpSound => pickUpSound;

        [SerializeField] bool useInventory;
        public bool UseInventory => useInventory;

        [SerializeField] int moneyConversionRate;
        public int MoneyConversionRate => moneyConversionRate;

        private PoolGeneric<FlyingResourceBehavior> flyingResPool;
        public PoolGeneric<FlyingResourceBehavior> FlyingResPool => flyingResPool;

        private Pool dropResPool;
        public Pool DropResPool => dropResPool;

        public void Init(Currency currency)
        {

        }

        public void InitPools(CurrencyType currencyType)
        {
            flyingResPool = new PoolGeneric<FlyingResourceBehavior>(flyingResPrefab, $"Flying_{currencyType}");
            dropResPool = new Pool(dropResPrefab, $"Drop_{currencyType}");
        }

        public void Unload()
        {
            flyingResPool.Destroy();
            flyingResPool = null;

            dropResPool.Destroy();
            dropResPool = null;
        }
    }
}