namespace Arcana.AgentsNet.Examples.DTOs
{
  #region DTOs para APIs

  public class BrapiResponse
  {
    public BrapiStockQuote[] Results { get; set; }
  }

  public class BrapiStockQuote
  {
    public string Symbol { get; set; }
    public string LongName { get; set; }
    public double RegularMarketPrice { get; set; }
    public double RegularMarketChange { get; set; }
    public double RegularMarketChangePercent { get; set; }
    public long RegularMarketVolume { get; set; }
    public long MarketCap { get; set; }
    public double PriceEarningsRatio { get; set; }
    public double PriceToBookRatio { get; set; }
    public double EarningsPerShare { get; set; }
    public double DividendYield { get; set; }
    public string Sector { get; set; }
    public string Industry { get; set; }
    public HistoricalPrice[] HistoricalDataPrice { get; set; }
  }

  public class HistoricalPrice
  {
    public long Date { get; set; }
    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }
    public long Volume { get; set; }
  }

  public class IBGEEstado
  {
    public string Nome { get; set; }
    public IBGERegiao Regiao { get; set; }
  }

  public class IBGERegiao
  {
    public string Nome { get; set; }
  }

  public class BCBSerie
  {
    public string Data { get; set; }
    public string Valor { get; set; }
  }

  public class IBGEPopulacao
  {
    public IBGEProjecaoPopulacao ProjecaoPopulacao { get; set; }
  }

  public class IBGEProjecaoPopulacao
  {
    public long PopulacaoProjetada { get; set; }
  }

  #endregion
}
