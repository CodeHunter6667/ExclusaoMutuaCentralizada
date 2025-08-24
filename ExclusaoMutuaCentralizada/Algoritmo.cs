using System.Data;
using System.Threading.Tasks;

namespace ExclusaoMutuaCentralizada;
public class Algoritmo
{
    Random random = new Random();

    public int QuantidadeNos { get; set; }
    private Queue<No> FilaNos { get; set; } = new Queue<No>(); // Fila de requisição

    private List<No> ListaNos { get; set; } = new List<No>(); // Lista de processos existentes

    private int ContadorNos = 0;

    private readonly object _lock = new object();

    private No? _coordenadorAtual;

    private bool recursoEmUso = false; // Indica se o recurso está ocupado

    private CancellationTokenSource cts = new CancellationTokenSource();

    public void Start()
    {
        int NumeroRandomico = random.Next(0, QuantidadeNos);

        for (int i = 0; i < QuantidadeNos; i++)
        {
            ListaNos.Add(new No
            {
                Id = Guid.NewGuid(),
                NomeProcesso = $"Processo: {i}",
                Coordenador = NumeroRandomico == i
            });
            ContadorNos++;
        }

        // Inicia o processamento da fila em background
        Task.Run(() => ProcessarFila(), cts.Token);
    }

    private void ProcessarFila()
    {
        while (!cts.Token.IsCancellationRequested)
        {
            No? noParaProcessar = null;

            lock (_lock)
            {
                if (!recursoEmUso && FilaNos.Count > 0)
                {
                    noParaProcessar = FilaNos.Dequeue();
                    recursoEmUso = true;
                }
            }

            if (noParaProcessar != null)
            {
                Console.WriteLine($"Processando requisição do {noParaProcessar.NomeProcesso}");
                while (DateTime.Now < noParaProcessar.SegundosProcessamento)
                {
                    Thread.Sleep(50); // Simula execução na seção crítica
                }
                Console.WriteLine($"Requisição do {noParaProcessar.NomeProcesso} concluída");

                lock (_lock)
                {
                    recursoEmUso = false;
                }
            }
            else
            {
                Thread.Sleep(50); // Aguarda antes de tentar processar próximo
            }
        }
    }

    // Método para criação de requisição: sempre enfileira a requisição
    private void CriarRequisicao(No noRequisitor)
    {
        lock (_lock)
        {
            Console.WriteLine($"Recebida requisição de {noRequisitor.NomeProcesso}");
            FilaNos.Enqueue(noRequisitor);
        }
    }

    // Outros métodos do código original permanecem iguais...

    private void CriarNovoNo()
    {
        ListaNos.Add(new No
        {
            Id = Guid.NewGuid(),
            NomeProcesso = $"Processo: {ContadorNos}",
            Coordenador = false
        });
        ContadorNos++;
        QuantidadeNos++;
    }

    private void LimpaFilaNos()
    {
        lock (_lock)
        {
            FilaNos.Clear();
        }
    }

    private void DefinirNovoCoordenador()
    {
        var novoCoordenador = ListaNos.OrderBy(x => Guid.NewGuid()).First();
        novoCoordenador.Coordenador = true;
    }

    private void CoordenadorMorre()
    {
        var noCoordenador = ListaNos.Where(x => x.Coordenador).FirstOrDefault();
        if (noCoordenador != null)
        {
            ListaNos.Remove(noCoordenador);
            QuantidadeNos--;
        }
    }

    private class No
    {
        public Guid Id { get; set; }
        public string NomeProcesso { get; set; } = string.Empty;
        public bool Coordenador { get; set; }
        public DateTime SegundosProcessamento { get; set; }

        public No()
        {
            NovoTempoProcessamento();
        }

        public void NovoTempoProcessamento()
        {
            var novoProcessamento = DateTime.Now.AddSeconds(new Random().Next(5, 15));
            SegundosProcessamento = novoProcessamento;
        }
    }
}

