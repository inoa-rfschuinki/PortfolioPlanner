using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Xml;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace PortfolioPlanner
{
    // ── ML.NET types ─────────────────────────────────────────────────────────
    public class SentimentInput
    {
        [LoadColumn(0)] public string Text { get; set; } = "";
        [LoadColumn(1), ColumnName("Label")] public bool IsPositive { get; set; }
    }
    public class SentimentOutput
    {
        [ColumnName("PredictedLabel")] public bool IsPositive { get; set; }
    }

    // ── Domain models ─────────────────────────────────────────────────────────
    public class NewsItem
    {
        public string PubDate    { get; set; } = "";
        public string Title      { get; set; } = "";
        public string Asset      { get; set; } = "-";
        public string Sentiment  { get; set; } = "Neutro";
        public bool   IsPositive { get; set; }
    }

    public class PortfolioItem
    {
        public string Asset          { get; set; } = "";
        public string Category       { get; set; } = "";
        public double WeightPct      { get; set; }
        public double AllocatedValue { get; set; }
        public string Suggestion     { get; set; } = "";
    }

    public class AssetDef
    {
        public string   Ticker    { get; set; } = "";
        public string   Category  { get; set; } = "";
        public string[] Keywords  { get; set; } = [];
    }

    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<string>        _feeds     = new();
        private readonly ObservableCollection<NewsItem>      _news      = new();
        private readonly ObservableCollection<PortfolioItem> _portfolio = new();

        // ── Asset universe (20 popular B3 assets) ────────────────────────────
        private static readonly List<AssetDef> Assets = new()
        {
            new() { Ticker = "PETR4",  Category = "Renda Variavel", Keywords = ["petrobras", "petr4", "petr3", "petróleo", "petroleo", "pré-sal", "pre-sal", "combustível", "gasolina", "diesel", "estatal", "refinaria"] },
            new() { Ticker = "VALE3",  Category = "Renda Variavel", Keywords = ["vale", "vale3", "minério", "minerio", "minério de ferro", "siderurgia", "pelotas", "commodities metálicas"] },
            new() { Ticker = "ITUB4",  Category = "Renda Variavel", Keywords = ["itau", "itaú", "itub4", "itub3", "unibanco", "banco itaú", "itaú unibanco"] },
            new() { Ticker = "BBDC4",  Category = "Renda Variavel", Keywords = ["bradesco", "bbdc4", "bbdc3", "banco bradesco", "agora investimentos", "bram"] },
            new() { Ticker = "BBAS3",  Category = "Renda Variavel", Keywords = ["banco do brasil", "bbas3", "bb", "banco estatal", "agronegócio bb"] },
            new() { Ticker = "ABEV3",  Category = "Renda Variavel", Keywords = ["ambev", "abev3", "inbev", "cervejaria", "brahma", "skol", "antarctica", "setor de bebidas"] },
            new() { Ticker = "WEGE3",  Category = "Renda Variavel", Keywords = ["weg", "wege3", "motores elétricos", "equipamentos elétricos", "weg s.a.", "energia solar weg"] },
            new() { Ticker = "EMBR3",  Category = "Renda Variavel", Keywords = ["embraer", "embr3", "aviação", "jatos", "aeronaves", "eve", "evtol", "setor aéreo"] },
            new() { Ticker = "RENT3",  Category = "Renda Variavel", Keywords = ["localiza", "rent3", "aluguel de carros", "locação de veículos", "unidas", "seminovos localiza"] },
            new() { Ticker = "MGLU3",  Category = "Renda Variavel", Keywords = ["magazine luiza", "magalu", "mglu3", "varejo online", "e-commerce", "luiza trajano", "varejista"] },
            new() { Ticker = "SUZB3",  Category = "Renda Variavel", Keywords = ["suzano", "suzb3", "papel e celulose", "celulose", "fibria", "exportação de papel"] },
            new() { Ticker = "GGBR4",  Category = "Renda Variavel", Keywords = ["gerdau", "ggbr4", "aço", "siderurgia", "metalurgia", "vergalhões"] },
            new() { Ticker = "B3SA3",  Category = "Renda Variavel", Keywords = ["b3sa3", "bolsa de valores", "b3", "ibovespa", "bovespa", "b3 s.a.", "mercado financeiro", "cetip"] },
            new() { Ticker = "TOTS3",  Category = "Renda Variavel", Keywords = ["totvs", "tots3", "software", "tecnologia", "erp", "sistemas de gestão", "tech brasileira"] },
            new() { Ticker = "RADL3",  Category = "Renda Variavel", Keywords = ["raia drogasil", "radl3", "rd saude", "rd saúde", "farmácia", "drogaria", "drogasil", "droga raia", "varejo farmacêutico"] },
            new() { Ticker = "LREN3",  Category = "Renda Variavel", Keywords = ["lojas renner", "renner", "lren3", "varejo de moda", "vestuário", "camicado", "youcom", "realize crédito"] },
            new() { Ticker = "RDOR3",  Category = "Renda Variavel", Keywords = ["rede dor", "rede d'or", "rdor3", "hospitais", "saúde", "são luiz", "sulamerica", "sul américa", "setor hospitalar"] },
            new() { Ticker = "ELET3",  Category = "Renda Variavel", Keywords = ["eletrobras", "elet3", "elet6", "energia", "elétrica", "setor elétrico", "geração de energia", "transmissão eletrobras"] },
            new() { Ticker = "TAEE11", Category = "Outros",         Keywords = ["taesa", "taee11", "taee3", "taee4", "transmissão de energia", "linhas de transmissão", "setor elétrico"] },
            new() { Ticker = "MXRF11", Category = "Outros",         Keywords = ["maxi renda", "mxrf11", "fii", "fundo imobiliario", "fundo imobiliário", "fundos de papel", "dividendos fii", "ifix"] }
        };

        // ── Static Renda Fixa bucket (simulated) ──────────────────────────────
        private static readonly List<AssetDef> FixedIncomeAssets = new()
        {
            new() { Ticker = "CDB_IPCA",   Category = "Renda Fixa", Keywords = ["cdb", "certificado de depósito bancário", "cdb atrelado", "cdb ipca", "cdb prefixado", "renda fixa privada"] },
            new() { Ticker = "LTN",        Category = "Renda Fixa", Keywords = ["tesouro prefixado", "ltn", "título público prefixado", "juros prefixados"] },
            new() { Ticker = "NTN_B",      Category = "Renda Fixa", Keywords = ["tesouro ipca", "tesouro ipca+", "ntn-b", "ntnb", "título atrelado à inflação", "proteção contra inflação"] },
            new() { Ticker = "LFT",        Category = "Renda Fixa", Keywords = ["tesouro selic", "lft", "taxa selic", "reserva de emergência", "juros pós-fixados"] },
            new() { Ticker = "DEBENTURES", Category = "Renda Fixa", Keywords = ["debenture", "debentures", "debêntures", "debênture incentivada", "crédito privado", "emissão de dívida", "títulos corporativos"] }
        };

        // ── ML.NET prediction engine ──────────────────────────────────────────
        private readonly PredictionEngine<SentimentInput, SentimentOutput> _predictor;

        public MainWindow()
        {
            InitializeComponent();

            FeedsListBox.ItemsSource     = _feeds;
            NewsDataGrid.ItemsSource     = _news;
            PortfolioDataGrid.ItemsSource = _portfolio;

            ProfileComboBox.ItemsSource     = new[] { "Conservador", "Moderado", "Arrojado" };
            ProfileComboBox.SelectedIndex   = 1;

            // Feeds fornecidos inicialmente
            _feeds.Add("https://feeds.folha.uol.com.br/mercado/rss091.xml");
            _feeds.Add("https://www.infomoney.com.br/feed/");

            // Economia, Mercado Financeiro e Negócios
            _feeds.Add("https://valor.globo.com/rss/");
            _feeds.Add("https://g1.globo.com/rss/g1/economia/");
            _feeds.Add("http://rss.uol.com.br/feed/economia.xml");
            _feeds.Add("https://exame.com/feed/");
            _feeds.Add("https://investnews.com.br/feed/");
            _feeds.Add("https://www.suno.com.br/noticias/feed/");
            _feeds.Add("https://neofeed.com.br/feed/");
            _feeds.Add("https://www.moneytimes.com.br/feed/");
            _feeds.Add("https://www.cnnbrasil.com.br/economia/feed/");
            _feeds.Add("https://einvestidor.estadao.com.br/feed/");
            _feeds.Add("https://agenciabrasil.ebc.com.br/economia/feed/");
            _feeds.Add("https://epocanegocios.globo.com/rss/epocanegocios/");
            _feeds.Add("https://www.jota.info/feed");
            _feeds.Add("https://www.cartacapital.com.br/economia/feed/");

            // Notícias Gerais (Brasil e Mundo) com forte impacto em mercado
            _feeds.Add("https://g1.globo.com/rss/g1/");
            _feeds.Add("https://feeds.folha.uol.com.br/emcimadahora/rss091.xml");
            _feeds.Add("http://rss.uol.com.br/feed/noticias.xml");
            _feeds.Add("https://www.cnnbrasil.com.br/feed/");
            _feeds.Add("https://feeds.bbci.co.uk/portuguese/rss.xml");
            _feeds.Add("https://agenciabrasil.ebc.com.br/rss/ultimasnoticias/feed");
            _feeds.Add("https://www.estadao.com.br/rss/ultimas.xml");

            // Tecnologia e Inovação (que frequentemente movem mercados de Tech)
            _feeds.Add("https://tecnoblog.net/feed/");
            _feeds.Add("https://canaltech.com.br/rss/");
            _feeds.Add("https://www.tecmundo.com.br/rss");
            _feeds.Add("https://gizmodo.uol.com.br/feed/");

            // Fundos de Investimentos, FIIs e Portais Focados em Alocação
            _feeds.Add("https://maisretorno.com/portal/feed");
            _feeds.Add("https://www.seudinheiro.com/feed/");
            _feeds.Add("https://euqueroinvestir.com/feed/");
            _feeds.Add("https://www.infomoney.com.br/onde-investir/fundos-de-investimento/feed/");
            _feeds.Add("https://www.infomoney.com.br/onde-investir/fundos-imobiliarios/feed/");
            _feeds.Add("https://fiis.com.br/feed/");
            _feeds.Add("https://www.suno.com.br/noticias/fiis/feed/");
            _feeds.Add("https://www.spacemoney.com.br/feed/");
            _feeds.Add("https://www.clubedospoupadores.com/feed");

            _predictor = BuildMlPredictor();
        }

        // ── ML.NET: train a binary sentiment classifier with inline data ──────
        private static PredictionEngine<SentimentInput, SentimentOutput> BuildMlPredictor()
        {
            var mlContext = new MLContext(seed: 42);

            var trainData = new List<SentimentInput>
            {
                new() { Text = "lucro recorde supera expectativas", IsPositive = true },
                new() { Text = "crescimento forte resultado positivo", IsPositive = true },
                new() { Text = "expansao avanco contrato aprovado", IsPositive = true },
                new() { Text = "alta valoriza ganho dividendo", IsPositive = true },
                new() { Text = "melhora destaque recuperacao projecao", IsPositive = true },
                new() { Text = "parcerias estrategicas elevam receita", IsPositive = true },
                new() { Text = "bateu meta superou guidance mercado comemora", IsPositive = true },
                new() { Text = "investimento amplia capacidade producao", IsPositive = true },
                new() { Text = "acao dispara apos resultado acima esperado", IsPositive = true },
                new() { Text = "empresa anuncia novo contrato bilionario", IsPositive = true },
                new() { Text = "prejuizo queda crise perde corte", IsPositive = false },
                new() { Text = "reducao negativo alerta risco problema", IsPositive = false },
                new() { Text = "multa investigacao rescisao cancelamento atraso", IsPositive = false },
                new() { Text = "impacto revisao baixa perda declinio divida", IsPositive = false },
                new() { Text = "default calote reestruturacao passivo elevado", IsPositive = false },
                new() { Text = "cai fundo pior trimestre decepcionante", IsPositive = false },
                new() { Text = "acao despenca apos resultado abaixo esperado", IsPositive = false },
                new() { Text = "empresa enfrenta dificuldade financeira grave", IsPositive = false },
                new() { Text = "plano de demissoes corte custos emergencial", IsPositive = false },
                new() { Text = "credores pressionam vencimento curto prazo", IsPositive = false },
                new() { Text = "mercado estavel sem novidades relevantes", IsPositive = true },
                new() { Text = "empresa mantem guidance e reafirma projecao", IsPositive = true },
                new() { Text = "setor aguarda decisao banco central", IsPositive = true },
            };

            var dataView = mlContext.Data.LoadFromEnumerable(trainData);

            var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentInput.Text))
                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: "Label", featureColumnName: "Features"));

            var model = pipeline.Fit(dataView);
            return mlContext.Model.CreatePredictionEngine<SentimentInput, SentimentOutput>(model);
        }

        // ── Feed management ───────────────────────────────────────────────────
        private void AddFeed_Click(object sender, RoutedEventArgs e)
        {
            var url = FeedUrlTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(url) && !_feeds.Contains(url))
            {
                _feeds.Add(url);
                FeedUrlTextBox.Clear();
            }
        }

        private void RemoveFeed_Click(object sender, RoutedEventArgs e)
        {
            if (FeedsListBox.SelectedItem is string sel)
                _feeds.Remove(sel);
        }

        // ── Fetch & classify news ─────────────────────────────────────────────
        private async void FetchNews_Click(object sender, RoutedEventArgs e)
        {
            _news.Clear();
            using var http = new HttpClient();
            http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "PortfolioPlanner/2.0");
            http.Timeout = System.TimeSpan.FromSeconds(15);

            foreach (var feedUrl in _feeds.ToList())
            {
                try
                {
                    var xml = await http.GetStringAsync(feedUrl);
                    using var reader = XmlReader.Create(new System.IO.StringReader(xml));
                    var feed = SyndicationFeed.Load(reader);

                    foreach (var item in feed.Items.Take(25))
                    {
                        var title   = item.Title?.Text ?? "";
                        var summary = item.Summary?.Text ?? "";
                        var text    = title + " " + summary;
                        var asset   = DetectAsset(text);
                        var pred    = _predictor.Predict(new SentimentInput { Text = text });

                        _news.Add(new NewsItem
                        {
                            PubDate    = item.PublishDate.ToLocalTime().ToString("dd/MM/yy HH:mm"),
                            Title      = title.Length > 110 ? title[..110] + "…" : title,
                            Asset      = asset,
                            IsPositive = pred.IsPositive,
                            Sentiment  = pred.IsPositive ? "Positivo" : "Negativo",
                        });
                    }
                }
                catch { /* feed unavailable or malformed */ }
            }
        }

        private static string DetectAsset(string text)
        {
            foreach (var a in Assets)
                foreach (var kw in a.Keywords)
                    if (text.Contains(kw, System.StringComparison.OrdinalIgnoreCase))
                        return a.Ticker;

            foreach (var a in FixedIncomeAssets)
                foreach (var kw in a.Keywords)
                    if (!string.IsNullOrEmpty(kw) &&
                        text.Contains(kw, System.StringComparison.OrdinalIgnoreCase))
                        return a.Ticker;

            return "-";
        }

        // ── Portfolio generation ──────────────────────────────────────────────
        private void GeneratePortfolio_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(BudgetTextBox.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double budget) || budget <= 0)
            {
                MessageBox.Show("Insira um Budget valido.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var profile = ProfileComboBox.SelectedItem?.ToString() ?? "Moderado";

            // Profile allocation ranges (target midpoint)
            var (rfPct, rvPct, otPct) = profile switch
            {
                "Conservador" => (0.80, 0.15, 0.05),
                "Arrojado"    => (0.30, 0.65, 0.05),
                _             => (0.55, 0.40, 0.05),   // Moderado
            };

            // Sentiment aggregation per ticker
            var sentimentMap = _news
                .Where(n => n.Asset != "-")
                .GroupBy(n => n.Asset)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count(n => n.IsPositive) - g.Count(n => !n.IsPositive));

            string Suggest(string ticker) =>
                !sentimentMap.TryGetValue(ticker, out int s) ? "Manter"
                : s > 0 ? "Comprar"
                : s < 0 ? "Vender"
                : "Manter";

            const double maxSingle = 0.10;

            _portfolio.Clear();

            AllocateBucket(Assets.Where(a => a.Category == "Renda Variavel").ToList(),
                           rvPct, budget, maxSingle);
            AllocateBucket(FixedIncomeAssets,
                           rfPct, budget, maxSingle);
            AllocateBucket(Assets.Where(a => a.Category == "Outros").ToList(),
                           otPct, budget, maxSingle);

            // Cash remainder
            double used = _portfolio.Sum(p => p.AllocatedValue);
            double cash = budget - used;
            if (cash > 0.01)
                _portfolio.Add(new PortfolioItem
                {
                    Asset = "CAIXA", Category = "Reserva",
                    WeightPct = cash / budget * 100,
                    AllocatedValue = cash, Suggestion = "Reserva"
                });

            void AllocateBucket(List<AssetDef> bucket, double bucketPct,
                                 double totalBudget, double cap)
            {
                var eligible = bucket
                    .Where(a => Suggest(a.Ticker) != "Vender")
                    .ToList();

                if (eligible.Count == 0)
                    eligible = bucket; // fallback: include all

                double each = System.Math.Min(bucketPct / eligible.Count, cap);

                foreach (var a in bucket)
                {
                    double w   = eligible.Contains(a) ? each : 0;
                    double val = totalBudget * w;
                    _portfolio.Add(new PortfolioItem
                    {
                        Asset          = a.Ticker,
                        Category       = a.Category,
                        WeightPct      = w * 100,
                        AllocatedValue = val,
                        Suggestion     = Suggest(a.Ticker)
                    });
                }
            }
        }
    }
}
