# 📊 Intelligent Portfolio Robo-Advisor: News Analytics & Systematic Asset Allocation

[![Express Experiment](https://img.shields.io/badge/Experiment-20_Minutes_/_%240.08-green?style=for-the-badge)](https://linkedin.com)
[![.NET Version](https://img.shields.io/badge/.NET-8.0-blue?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![ML.NET](https://img.shields.io/badge/ML.NET-NLP_Sentiment-purple?style=for-the-badge)](https://dotnet.microsoft.com/apps/machinelearning-ai/ml-dotnet)

Uma solução de **Investimento Sistemático** (*Systematic Portfolio Management*) e **Robo-Advisory** desenvolvida em C# com WPF. O sistema realiza a ingestão de **Dados Alternativos** (*Alternative Data*) através de feeds RSS, executa o reconhecimento de entidades contextuais para ativos da B3, analisa o sentimento do mercado via Processamento de Linguagem Natural (PLN) local com **ML.NET** e gera uma alocação automatizada e diversificada de ativos respeitando os limites regulatórios e perfis de risco (alinhados com as diretrizes de certificações como CPA).

> 🔬 **O Experimento:** Este projeto foi inteiramente concebido, estruturado e codificado do zero em **apenas 20 minutos**, gerando um custo computacional de **exatos US$ 0,08** em tokens de IA, utilizando o GitHub Copilot (operando o modelo Claude Sonnet). O objetivo foi provar a eficiência brutal e a viabilidade econômica do desenvolvimento acelerado por inteligência artificial pragmática.

---

## 🚀 Funcionalidades Principais

* **News Analytics & RSS Manager:** Módulo nativo para cadastro, edição e remoção de fontes de notícias RSS (`System.ServiceModel.Syndication`).
* **Contextual Entity Recognition (NER):** Varredura e mapeamento automático através de palavras-chave estruturadas para associar manchetes de notícias a ativos específicos da B3 e títulos de Renda Fixa.
* **PLN Local (Machine Learning):** Pipeline de análise de sentimento binária integrado via **ML.NET**, rodando 100% *on-premise* (sem chamadas de API externas), classificando o impacto das notícias como Positivo (Sinal de Compra/Manutenção) ou Negativo (Sinal de Alerta/Venda).
* **Motor de Alocação Quantitativa:** Distribuição automatizada de um capital investido (*Budget*) baseado em três perfis de risco, aplicando filtros de diversificação rigorosos (teto máximo de 10% de exposição por ativo para mitigação de risco).

---

## 📈 Universo de Ativos Suportados (B3 & Renda Fixa)

O motor rastreia e pondera uma carteira base de **25 ativos**, divididos estrategicamente para atender aos limites de alocação de cada perfil:

### Renda Variável (Ações B3) & Outros (FIIs/Energia)
* `PETR4` (Petrobras), `VALE3` (Vale), `ITUB4` (Itaú), `BBDC4` (Bradesco), `BBAS3` (Banco do Brasil).
* `ABEV3` (Ambev), `WEGE3` (Weg), `EMBR3` (Embraer), `RENT3` (Localiza), `MGLU3` (Magalu).
* `SUZB3` (Suzano), `GGBR4` (Gerdau), `B3SA3` (B3), `TOTS3` (Totvs), `RADL3` (RD Saúde).
* `LREN3` (Lojas Renner), `RDOR3` (Rede D'Or), `ELET3` (Eletrobras).
* `TAEE11` (Taesa - Outros), `MXRF11` (Maxi Renda FII - Outros).

### Renda Fixa (Títulos Públicos e Privados)
* `CDB_IPCA` (Certificado de Depósito Bancário Privado)
* `LTN` (Tesouro Prefixado)
* `NTN_B` (Tesouro IPCA+)
* `LFT` (Tesouro Selic)
* `DEBENTURES` (Crédito Privado Corporativo)

---

## 🛠️ Regras de Alocação por Perfil de Risco

O algoritmo impõe de forma estrita as matrizes de diversificação do mercado financeiro, balanceando as classes de acordo com a escolha do usuário:

| Perfil | Renda Fixa | Renda Variável | Outros (FIIs) |
| :--- | :--- | :--- | :--- |
| **Conservador** | 70% – 90% | 10% – 30% | Até 10% |
| **Moderado** | 40% – 70% | 30% – 60% | Até 15% |
| **Arrojado** | 20% – 50% | 50% – 80% | Até 20% |

**Mecânica do Algoritmo:**
1.  O sistema valida o *Budget Total (PL)* inserido.
2.  Filtra os ativos da categoria alvo baseando-se no sentimento das notícias vigentes coletadas no pipeline de PLN.
3.  Ativos com sentimento majoritariamente positivo recebem prioridade de peso. Ativos com sinal negativo têm sua exposição zerada ou minimizada.
4.  Aplica o limite máximo de **10% do PL total por ativo** para garantir a diluição do risco de ruína.

---

## 💻 Arquitetura Técnico & Stack

A aplicação adota uma abordagem minimalista de alto desempenho, consolidando a interface e o processamento de regras no modelo *Code-Behind* otimizado para reduzir sobrecarga de arquivos e consumo de memória:

* **UI/Apresentação:** XAML (WPF) configurado sob paleta sóbria (Dark/Slate Theme) otimizada para terminais financeiros comerciais.
* **Linguagem & Runtime:** C# 12 / .NET 8.0.
* **Inteligência Artificial:** `Microsoft.ML` (ML.NET) utilizando `PredictionEngine` para inferência de texto contextualizado.
* **Sindicância de Dados:** `System.ServiceModel.Syndication` para parsing XML eficiente e assíncrono de feeds de notícias.

---

## ⚙️ Como Executar o Projeto

### Pré-requisitos
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado.
* Visual Studio 2022 (ou VS Code com extensões C# e Dev Kit).

### Passos para Execução
1. Clone o repositório em sua máquina local:
   ```bash
   git clone [https://github.com/seu-usuario/intelligent-portfolio-robo-advisor.git](https://github.com/seu-usuario/intelligent-portfolio-robo-advisor.git)
2. Acesse o diretório do projeto:
   ```bash
   cd intelligent-portfolio-robo-advisor
3.Restaure as dependências do NuGet (incluindo o pacote do ML.NET):
   ```bash
   dotnet restore
4. Compile e execute a aplicação:
   ```bash
   dotnet run

## 📄 Licença
Este projeto é um experimento educacional livre. Sinta-se à vontade para clonar, modificar e expandir o motor de alocação quântica conforme suas necessidades de estudo.
