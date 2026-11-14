using UnityEngine;

public class TowerPurchase : MonoBehaviour
{
    public int purchasePrice;
    public int evolutionSpend;
    public int TotalSpent => purchasePrice + evolutionSpend;
    public bool Sold { get; private set; }
    public static int RefundFor(int price) => Mathf.Max(1, price / 2);
    public bool TrySell(EnemySpawner wallet)
    {
        if (Sold || wallet == null || wallet.State != EnemySpawner.WaveState.PreWave) return false;
        Sold = true;
        wallet.AddCoins(RefundFor(TotalSpent));
        gameObject.SetActive(false);
        Destroy(gameObject);
        return true;
    }
}
