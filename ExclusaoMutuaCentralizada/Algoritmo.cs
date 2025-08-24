using System.Data;

namespace ExclusaoMutuaCentralizada;
public class Algoritmo

{

    Random random = new Random();

    public int QuantidadeNos { get; set; }
    private Stack<No> PilhaNos { get; set; } = new Stack<No>(); //Fila de requisição

    private List<No> ListaNos { get; set; } = new List<No>(); // Lista de processos existentes

    private int ContadorNos = 0;

    public void Start()
    {
        int NumeroRandomico = random.Next(1, QuantidadeNos);

        for (int i = 0; i <= QuantidadeNos; i++)
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
        var limiteCoordenador = DateTime.Now.AddSeconds(60);
        while (DateTime.Now.TimeOfDay < limiteCoordenador.TimeOfDay) //Enquanto o coordenador está vivo
        {
            var limiteCriaProcesso = DateTime.Now.AddSeconds(40);
            while(DateTime.Now.TimeOfDay < limiteCriaProcesso.TimeOfDay) //Enquanto um processo novo não é criado
            {
                for(int i = 0; i <= QuantidadeNos; i++)
                {
                    while (DateTime.Now.TimeOfDay < ListaNos[i].SegundosRequisicao.TimeOfDay) //Enquanto o tempo de requisição do nó não for atingido
                    {
                        //Pula o tempo
                    }
                    CriarRequisicao(ListaNos[i]); //Chamada da criação de requisição
                    ProcessarRequisicao(ListaNos[i]); //Chamada do processamento de requisição
                }
            }
            CriarNovoNo();
        }

        //Coordenador atual morre e a pilha de requisições é perdida
        CoordenadorMorre();
        LimpaPilhaNos();
        //Um novo coordenador é definido aleatoriamente
        DefinirNovoCoordenador();

        //O programa começa um novo ciclo
        Processar();
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

            }
        }
        else // Existe um processo na fila, o qual deve ser removido e processado
        {
            No processoNaFila = PilhaNos.Pop();
            while (DateTime.Now.TimeOfDay < noRequisidor.SegundosProcessamento.TimeOfDay) // Equanto o processo estiver consumindo o recurso
            {

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

    private void LimpaPilhaNos()
    {
        PilhaNos.Clear();
    }

    private void DefinirNovoCoordenador()
    {
        var novoCoordenador = ListaNos.OrderBy(x => Guid.NewGuid()).First(); //O novo coordenador é aquele na lista com o menor ID
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
        public string NomeProcesso { get; set; }

        public bool Coordenador { get; set; }

        public DateTime SegundosProcessamento { get; set; }
        public DateTime SegundosRequisicao { get; set; }

        public No()
        {
            Random random = new Random();

            SegundosRequisicao = DateTime.Now.AddSeconds(random.Next(10, 25)); // A cada 10-25 segundos o nó solicita acesso ao recurso
            SegundosProcessamento = DateTime.Now.AddSeconds(random.Next(5, 15)); // A cada 5-15 segundos o recurso é processado pelo nó

        }

    }
}
