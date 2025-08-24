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
            while (DateTime.Now.TimeOfDay < limiteCriaProcesso.TimeOfDay) //Enquanto um processo novo não é criado
            {

            }
            CriarNovoNo();
        }

        //Coordenador atual morre e a pilha de requisições é perdida
        CoordenadorMorre();
        LimpaPilhaNos();
        //Um novo coordenador é definido aleatoriamente
        DefinirNovoCoordenador();

        Processar();
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
        var novoCoordenador = ListaNos.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
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
