class Program
{
    static async Task Main(string[] args)
    {
        // Inicia o algoritmo com 3 processos iniciais
        var algoritmo = new Algoritmo(3);
        await algoritmo.IniciarAsync();
    }
}