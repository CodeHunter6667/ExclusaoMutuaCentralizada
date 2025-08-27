using System.Collections.Concurrent;

public class Algoritmo
{
    private readonly object travaRandom = new object();
    private readonly Random random = new Random();

    // Lista de nós (processos)
    private readonly ConcurrentDictionary<Guid, No> listaNos = new ConcurrentDictionary<Guid, No>();

    // Fila centralizada de requisições
    private readonly ConcurrentQueue<No> filaRequisicoes = new ConcurrentQueue<No>();

    private CancellationTokenSource cancelamento = new CancellationTokenSource();

    private No coordenador; // nó que age como coordenador
    private readonly int tempoVidaCoordenadorSeg = 60;  // coordenador vive 60s
    private readonly int intervaloCriarNoSeg = 40;      // cria novo nó a cada 40s

    public Algoritmo(int quantidadeInicial)
    {
        // Cria nós iniciais
        for (int i = 0; i < quantidadeInicial; i++)
        {
            AdicionarNo($"Processo {i}");
        }
        DefinirNovoCoordenadorAleatorio();
    }

    public async Task IniciarAsync()
    {
        Console.WriteLine("Algoritmo de Exclusão Mútua Centralizada iniciado.\n");

        // Loop do coordenador (atende fila de requisições)
        var tarefaCoordenador = Task.Run(() => LoopCoordenadorAsync(cancelamento.Token));

        // Tarefa que mata o coordenador a cada 60s
        var tarefaMatarCoordenador = Task.Run(async () =>
        {
            while (!cancelamento.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(tempoVidaCoordenadorSeg), cancelamento.Token)
                          .ContinueWith(_ => { });
                if (cancelamento.IsCancellationRequested) break;

                Console.WriteLine($"\n--- Coordenador ({coordenador?.Nome}) MORREU ---");

                // Remove coordenador
                if (coordenador != null)
                {
                    listaNos.TryRemove(coordenador.Id, out _);
                }

                // Limpa a fila de requisições
                LimparFila();
                coordenador = null;

                // Define um novo coordenador aleatoriamente
                if (listaNos.Count > 0)
                {
                    DefinirNovoCoordenadorAleatorio();
                    Console.WriteLine($"Novo coordenador: {coordenador.Nome}");
                }
                else
                {
                    Console.WriteLine("Não há nós vivos para eleger coordenador.");
                }
            }
        });

        // Tarefa que cria novos nós a cada 40s
        var tarefaCriarNos = Task.Run(async () =>
        {
            while (!cancelamento.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(intervaloCriarNoSeg), cancelamento.Token)
                          .ContinueWith(_ => { });
                if (cancelamento.IsCancellationRequested) break;

                var novo = AdicionarNo();
                Console.WriteLine($"\n[CRIADO] {novo.Nome}");

                // Inicia o loop do novo nó
                _ = Task.Run(() => LoopNoAsync(novo, cancelamento.Token));
            }
        });

        // Inicia loops para os nós existentes
        foreach (var no in listaNos.Values)
        {
            _ = Task.Run(() => LoopNoAsync(no, cancelamento.Token));
        }

        Console.WriteLine("Pressione ENTER para encerrar.");
        Console.ReadLine();

        // Encerramento
        cancelamento.Cancel();
        await Task.WhenAll(tarefaMatarCoordenador, tarefaCriarNos).ContinueWith(_ => { });
        Console.WriteLine("Execução finalizada.");
    }

    // Adiciona um novo nó (não-coordenador)
    private No AdicionarNo(string nome = null)
    {
        var novo = new No
        {
            Id = Guid.NewGuid(),
            Nome = nome ?? $"Processo {listaNos.Count}"
        };
        listaNos.TryAdd(novo.Id, novo);
        return novo;
    }

    // Loop de cada nó: tenta requisitar recurso de tempos em tempos
    private async Task LoopNoAsync(No no, CancellationToken token)
    {
        while (!token.IsCancellationRequested && listaNos.ContainsKey(no.Id))
        {
            // intervalo aleatório de 10–25s antes de pedir recurso novamente
            var espera = NumeroAleatorio(10, 25);
            await Task.Delay(TimeSpan.FromSeconds(espera), token).ContinueWith(_ => { });

            if (token.IsCancellationRequested || !listaNos.ContainsKey(no.Id)) break;

            // Se for coordenador, ele não solicita recurso
            if (coordenador != null && no.Id == coordenador.Id) continue;

            // Solicita acesso ao recurso
            FazerRequisicao(no);
        }
    }

    // Um nó solicita o recurso ao coordenador
    private void FazerRequisicao(No no)
    {
        // Sem coordenador → requisição perdida
        if (coordenador == null)
        {
            Console.WriteLine($" --> Sem coordenador. {no.Nome} perdeu sua requisição.");
            return;
        }

        // Se já está na fila, não pode enviar de novo
        if (filaRequisicoes.Any(x => x.Id == no.Id))
        {
            Console.WriteLine($" --> {no.Nome} já está aguardando na fila, não pode solicitar novamente.");
            return;
        }

        filaRequisicoes.Enqueue(no);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {no.Nome} entrou na fila.");
    }

    // Loop do coordenador: processa fila de requisições
    private async Task LoopCoordenadorAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (coordenador == null)
            {
                await Task.Delay(200, token).ContinueWith(_ => { });
                continue;
            }

            if (filaRequisicoes.TryDequeue(out var requisitante))
            {
                // Nó pode ter morrido nesse meio tempo
                if (!listaNos.ContainsKey(requisitante.Id)) continue;

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Coordenador ({coordenador.Nome}) concedeu recurso a {requisitante.Nome}");

                // Tempo de processamento de 5–15s
                int tempo = NumeroAleatorio(5, 15);
                Console.WriteLine($"   --> {requisitante.Nome} usando recurso por {tempo}s.");
                await Task.Delay(TimeSpan.FromSeconds(tempo), token).ContinueWith(_ => { });

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {requisitante.Nome} liberou o recurso.");
            }
            else
            {
                await Task.Delay(200, token).ContinueWith(_ => { });
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

    // Escolhe novo coordenador aleatoriamente
    private void DefinirNovoCoordenadorAleatorio()
    {
        if (listaNos.IsEmpty) return;

        var ids = listaNos.Keys.ToArray();
        var idx = NumeroAleatorio(0, ids.Length - 1);
        var id = ids[idx];
        listaNos.TryGetValue(id, out var escolhido);
        coordenador = escolhido;
        Console.WriteLine($"(Novo coordenador escolhido aleatoriamente: {coordenador.Nome})");
    }

    // Limpa toda a fila
    private void LimparFila()
    {
        while (filaRequisicoes.TryDequeue(out _)) { }
        Console.WriteLine("Fila de requisições foi limpa.");
    }

    // Sorteio de números com thread-safe
    private int NumeroAleatorio(int min, int max)
    {
        lock (travaRandom)
        {
            return random.Next(min, max + 1);
        }
    }
}

public class No
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
}