using System.Data;

namespace ExclusaoMutuaCentralizada;
public class Algoritmo
{
    Random random = new Random();

    public int QuantidadeNos { get; set; }
    private Queue<No> FilaNos { get; set; } = new Queue<No>(); // Fila de requisição

    private List<No> ListaNos { get; set; } = new List<No>(); // Lista de processos existentes

    private int ContadorNos = 0;

    public void Start()
    {
        int NumeroRandomico = random.Next(1, QuantidadeNos);

        for (int i = 0; i < QuantidadeNos; i++)
        {
            ListaNos.Add(new No
            {
                Id = Guid.NewGuid(),
                NomeProcesso = $"Processo: {i}",
                Coordenador = NumeroRandomico == i ? true : false  //Se o número randômico for igual ao i do laço for, define o nó como coordenador
            });
            ContadorNos++;
        }
    }

    public void Processar()
    {
        while (true)
        {
            var limiteCoordenador = DateTime.Now.AddSeconds(60);
            while (DateTime.Now < limiteCoordenador) //Enquanto o coordenador está vivo
            {
                var limiteCriaProcesso = DateTime.Now.AddSeconds(40);
                while (DateTime.Now < limiteCriaProcesso) //Enquanto um processo novo não é criado
                {
                    for (int i = 0; i < QuantidadeNos; i++)
                    {
                        while (DateTime.Now < ListaNos[i].SegundosRequisicao) //Enquanto o tempo de requisição do nó não for atingido
                        {
                            Thread.Sleep(50);
                        }
                        CriarRequisicao(ListaNos[i]); //Chamada da criação de requisição
                        ProcessarRequisicao(ListaNos[i]); //Chamada do processamento de requisição
                    }
                }
                CriarNovoNo();
            //Coordenador atual morre e a fila de requisições é perdida
            CoordenadorMorre();
            LimpaFilaNos();
            //Um novo coordenador é definido aleatoriamente
            DefinirNovoCoordenador();
        }
    }

    private void CriarRequisicao(No NoRequisitor)
    {
        var Coordenador = ListaNos.First(x => x.Coordenador); //A variável recebe o atual coordenador da fila. 

        if (FilaNos.Count == 0) //Se a fila estiver vazia, a requisição é atendida imediatamente.
        {
            Console.WriteLine("OK!");
            ProcessarRequisicao(NoRequisitor);
        }
        else //Se a fila tiver outros nós pendentes
        {
            FilaNos.Enqueue(NoRequisitor);
        }
    }

    private void ProcessarRequisicao(No noRequisidor)
    {
        if (FilaNos.Count == 0) //Se não há nada para processar, a requisição é atendida diretamente
        {
            while (DateTime.Now < noRequisidor.SegundosProcessamento) // Equanto o processo estiver consumindo o recurso
            {
                Thread.Sleep(50);
            }
        }
        else // Existe um processo na fila, o qual deve ser removido e processado
        {
            No processoNaFila = FilaNos.Dequeue();
            while (DateTime.Now < processoNaFila.SegundosProcessamento) // Equanto o processo estiver consumindo o recurso
            {
                Thread.Sleep(50);
            }
        }
    }

    private void CriarRequisicao(No NoRequisitor)
    {
        var Coordenador = ListaNos.First(x => x.Coordenador); //A variável recebe o atual coordenador da fila. 

        if(PilhaNos.Count == 0) //Se a fila estiver vazia, a requisição é atendida imediatamente.
        {
            Console.WriteLine("OK!");
            ProcessarRequisicao(NoRequisitor);
        }
        else //Se a fila tiver outros nós pendentes
        {
            PilhaNos.Push(NoRequisitor); //O nó requisitor é inserido na fila

        }
    }

    private void ProcessarRequisicao(No noRequisidor)
    {
        if(PilhaNos.Count == 0) //Se não há nada para processar, a requisição é atendida diretamente
        {
            while(DateTime.Now.TimeOfDay < noRequisidor.SegundosProcessamento.TimeOfDay) // Equanto o processo estiver consumindo o recurso
            {
                //Passou o tempo
            }
        }
        else // Existe um processo na fila, o qual deve ser removido e processado
        {
            No processoNaFila = PilhaNos.Pop();
            while (DateTime.Now.TimeOfDay < noRequisidor.SegundosProcessamento.TimeOfDay) // Equanto o processo estiver consumindo o recurso
            {
                //Passou o tempo          
            }
        }
    }

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
        FilaNos.Clear();
    }

    private void DefinirNovoCoordenador()
    {
        var novoCoordenador = ListaNos.OrderBy(x => Guid.NewGuid()).First(); //O novo coordenador é escolhido aleatoriamente
        novoCoordenador.Coordenador = true;
    }

    private void CoordenadorMorre()
    {
        var noCoordenador = ListaNos.Where(x => x.Coordenador).FirstOrDefault();
        ListaNos.Remove(noCoordenador);
        QuantidadeNos--;
    }

    private class No
    {
        public Guid Id { get; set; }
        public string NomeProcesso { get; set; } = string.Empty;
        public bool Coordenador { get; set; }
        public DateTime SegundosProcessamento {  get; set; }

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
