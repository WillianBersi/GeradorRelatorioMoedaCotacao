using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GeradorRelatorioMoedaCotacao
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cronometro = new Stopwatch();
            while (true)
            {
                cronometro.Start();
                await GerarRelatorio();
                cronometro.Stop();

                var ts = cronometro.Elapsed;
                string tempoDecorrido = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", 
                    ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                Console.WriteLine($"Tempo de execução do ciclo { tempoDecorrido }");
                cronometro.Reset();

                Thread.Sleep(TimeSpan.FromMinutes(2));               
            }
        }

        private static async Task GerarRelatorio() 
        {
            var client = new HttpClient { BaseAddress = new Uri("https://localhost:44382") };
            var response = await client.GetAsync("api/getitemfila");
            var content = await response.Content.ReadAsStringAsync();

            var moeda = JsonConvert.DeserializeObject<Moeda>(content);
            if (!string.IsNullOrEmpty(moeda.moeda))
            {
                var arrayMoeda = ObterArrayMoeda(moeda);
                var arrayMoedaCotacao = ObterArrayMoedaCotacao(arrayMoeda);

                var path = @"C:\dev\Resultado_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
                File.WriteAllLines(path, arrayMoedaCotacao);
                Console.WriteLine("Relatório gerado");
            }
            else
                Console.WriteLine("Não há dados");
        }

        private static Dictionary<string, string> ObterDicionarioMoedaCotacao()
        {
            return new Dictionary<string, string>()
             {
                {"AFN","66"},
                {"ALL","49"},
                {"ANG","33"},
                {"ARS","3"},
                {"AWG","6"},
                {"BOB","56"},
                {"BYN","64"},
                {"CAD","25"},
                {"CDF","58"},
                {"CLP","16"},
                {"COP","37"},
                {"CRC","52"},
                {"CUP","8"},
                {"CVE","51"},
                {"CZK","29"},
                {"DJF","36"},
                {"DZD","54"},
                {"EGP","12"},
                {"EUR","20"},
                {"FJD","38"},
                {"GBP","22"},
                {"GEL","48"},
                {"GIP","18"},
                {"HTG","63"},
                {"ILS","40"},
                {"IRR","17"},
                {"ISK","11"},
                {"JPY","9"},
                {"KES","21"},
                {"KMF","19"},
                {"LBP","42"},
                {"LSL","4"},
                {"MGA","35"},
                {"MGB","26"},
                {"MMK","69"},
                {"MRO","53"},
                {"MRU","15"},
                {"MUR","7"},
                {"MXN","41"},
                {"MZN","43"},
                {"NIO","23"},
                {"NOK","62"},
                {"OMR","34"},
                {"PEN","45"},
                {"PGK","2"},
                {"PHP","24"},
                {"RON","5"},
                {"SAR","44"},
                {"SBD","32"},
                {"SGD","70"},
                {"SLL","10"},
                {"SOS","61"},
                {"SSP","47"},
                {"SZL","55"},
                {"THB","39"},
                {"TRY","13"},
                {"TTD","67"},
                {"UGX","59"},
                {"USD","1"},
                {"UYU","46"},
                {"VES","68"},
                {"VUV","57"},
                {"WST","28"},
                {"XAF","30"},
                {"XAU","60"},
                {"XDR","27"},
                {"XOF","14"},
                {"XPF","50"},
                {"ZAR","65"},
                {"ZWL","31"},
              };
        }

        private static List<string> ObterArrayMoeda(Moeda moeda)
        {
            var reader = new StreamReader(File.OpenRead(@"C:\dev\DadosMoeda.csv"));
            List<string> lista = new List<string>();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');

                if (values[0] == moeda.moeda)
                {
                    var data = Convert.ToDateTime(values[1]);
                    var dataInicio = Convert.ToDateTime(moeda.Data_inicio);
                    var dataFim = Convert.ToDateTime(moeda.Data_fim);

                    if (data >= dataInicio && data <= dataFim)
                        lista.Add(line);
                }

            }
            return lista;
        }

        private static List<string> ObterArrayMoedaCotacao(List<string> arrayMoeda)
        {
            var dic = ObterDicionarioMoedaCotacao();
            var cotacao = "";
            var ListaMoedaCotacao = new List<string>();
            var moedaCotacao = "";

            foreach (var item in arrayMoeda)
            {
                var values = item.Split(';');
                foreach (KeyValuePair<string, string> itemDic in dic)
                {
                    if (values[0] == itemDic.Key)
                    {
                        cotacao = ObterCotacao(itemDic.Value, values[1]);
                        break;
                    }

                }
                moedaCotacao = item + ";" + cotacao;
                ListaMoedaCotacao.Add(moedaCotacao);
            }
            return ListaMoedaCotacao;
        }

        private static string ObterCotacao(string codCotacao, string moedaDataRef)
        {
            var reader = new StreamReader(File.OpenRead(@"C:\dev\DadosCotacao.csv"));
            List<string> lista = new List<string>();
            var moedaDataRefTratada = Convert.ToDateTime(moedaDataRef);

            var cotacao = "";
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');

                if (values[1] == codCotacao)
                {
                    var dataCotacao = Convert.ToDateTime(values[2]);
                    if (dataCotacao == moedaDataRefTratada)
                        cotacao = values[0];
                }
            }
            return cotacao;
        }
    }
}
