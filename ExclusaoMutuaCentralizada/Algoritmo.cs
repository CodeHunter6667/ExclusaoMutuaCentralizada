namespace ExclusaoMutuaCentralizada;
public class Algoritmo
{
    public int QuantidadeNos { get; set; }
    private Stack<No> PilhaNos { get; set; } = new Stack<No>();

    private List<No> ListaNos { get; set; } = new List<No>();

    public void Start()
    {
        Random random = new Random();
        int NumeroRandomico = random.Next(1, QuantidadeNos);

        for (int i = 0; i <= QuantidadeNos; i++)
        {
            ListaNos.Add(new No
            {
                Id = Guid.NewGuid(),
                NomeProcesso = $"Processo: {i}",
                Coordenador = NumeroRandomico == i ? true : false
            });
        }
    }

    public void Processar()
    {
        var limiteCoordenador = DateTime.Now.AddMinutes(1);

        while (DateTime.Now.TimeOfDay < limiteCoordenador.TimeOfDay)
        {
        }

        CoordenadorMorre();
        LimpaPilhaNos();


        Processar();
    }

    private void LimpaPilhaNos() 
    {
        PilhaNos.Clear();
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

    }
}
