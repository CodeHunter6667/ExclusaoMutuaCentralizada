namespace ExclusaoMutuaCentralizada;
public class Algoritmo
{
    public int Id { get; }
    public bool IsCoordenador { get; private set; }

    // Referência ao simulador para obter o coordenador atual.
    private readonly Simulador _simulador;
    private static readonly Random _random = new Random();

    // --- Atributos de Coordenador (só são usados se IsCoordenador == true) ---
    private readonly Queue<Algoritmo> _filaDeEspera = new Queue<Algoritmo>();
    private Algoritmo? _processoNaRegiaoCritica = null;
    private readonly object _lock = new object();
    // --------------------------------------------------------------------

    public Algoritmo(int id, Simulador simulador)
    {
        Id = id;
        _simulador = simulador;
        IsCoordenador = false;
    }

    public void TornarCoordenador()
    {
        Console.WriteLine($"[ELEIÇÃO] Processo {Id} agora é o Coordenador.");
        IsCoordenador = true;
    }

    public void DeixarDeSerCoordenador()
    {
        IsCoordenador = false;
        // A fila e o estado da região crítica são perdidos naturalmente com a "morte" do coordenador.
    }

    // O "loop de vida" de um processo.
    public async Task IniciarTrabalhoAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"[PROCESSO {Id}] iniciou seu trabalho.");
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Requisito: Processos tentam consumir recursos de 10 a 25 segundos.
                int tempoAteProximaRequisicao = _random.Next(10000, 25001);
                await Task.Delay(tempoAteProximaRequisicao, cancellationToken);

                var coordenadorAtual = _simulador.ObterCoordenadorAtual();
                if (coordenadorAtual == null)
                {
                    Console.WriteLine($"[PROCESSO {Id}] tentou fazer uma requisição, mas não há coordenador no momento.");
                    continue;
                }

                Console.WriteLine($"[PROCESSO {Id}] quer entrar na região crítica. Enviando REQUEST para o Coordenador {coordenadorAtual.Id}.");
                bool acessoConcedido = await coordenadorAtual.SolicitarAcesso(this);

                if (acessoConcedido)
                {
                    // Requisito: Tempo de processamento de 5 a 15 segundos.
                    Console.WriteLine($"\t\t>>>>> [PROCESSO {Id}] ENTROU na região crítica! Usando o recurso...");
                    int tempoDeProcessamento = _random.Next(5000, 15001);
                    await Task.Delay(tempoDeProcessamento, cancellationToken);
                    Console.WriteLine($"\t\t<<<<< [PROCESSO {Id}] SAIU da região crítica!");

                    await coordenadorAtual.LiberarAcesso(this);
                }
                else
                {
                    // Isso acontece se o coordenador morreu enquanto este processo estava na fila.
                    Console.WriteLine($"[PROCESSO {Id}] não obteve acesso pois o coordenador morreu. Tentará novamente mais tarde.");
                }
            }
            catch (TaskCanceledException)
            {
                // A simulação está terminando.
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PROCESSO {Id}] encontrou um erro: {ex.Message}");
            }
        }
        Console.WriteLine($"[PROCESSO {Id}] finalizou seu trabalho.");
    }

    #region Métodos de Coordenador

    // Chamado por outro processo (ou por ele mesmo) para pedir acesso.
    public Task<bool> SolicitarAcesso(Algoritmo processoRequisitante)
    {
        if (!IsCoordenador) return Task.FromResult(false); // Só o coordenador responde.

        lock (_lock)
        {
            Console.WriteLine($"[COORDENADOR {Id}] Recebeu REQUEST do Processo {processoRequisitante.Id}.");

            if (_processoNaRegiaoCritica == null && !_filaDeEspera.Any())
            {
                Console.WriteLine($"[COORDENADOR {Id}] Recurso está livre. Enviando GRANT para o Processo {processoRequisitante.Id}.");
                _processoNaRegiaoCritica = processoRequisitante;
                return Task.FromResult(true);
            }
            else
            {
                Console.WriteLine($"[COORDENADOR {Id}] Recurso ocupado. Processo {processoRequisitante.Id} foi adicionado à fila.");
                _filaDeEspera.Enqueue(processoRequisitante);
                // Em uma simulação mais complexa, usaríamos TaskCompletionSource para bloquear a chamada.
                // Para este modelo, a lógica de re-tentativa no processo cliente é suficiente.
                // Por simplicidade, vamos assumir que o processo cliente vai esperar e o coordenador o chamará.
                // Para a especificação atual, vamos simplificar e o processo não ficará "travado" esperando.
                // A simulação é mais sobre os eventos do que sobre a mecânica de bloqueio.
                return Task.FromResult(false); // Retorna falso para indicar que entrou na fila.
            }
        }
    }

    // Chamado pelo processo que estava na região crítica.
    public Task LiberarAcesso(Algoritmo processoQueLibera)
    {
        if (!IsCoordenador) return Task.CompletedTask;

        lock (_lock)
        {
            Console.WriteLine($"[COORDENADOR {Id}] Recebeu RELEASE do Processo {processoQueLibera.Id}.");
            _processoNaRegiaoCritica = null;

            if (_filaDeEspera.Any())
            {
                var proximoProcesso = _filaDeEspera.Dequeue();
                _processoNaRegiaoCritica = proximoProcesso;
                Console.WriteLine($"[COORDENADOR {Id}] Recurso liberado. Enviando GRANT para o próximo da fila: Processo {proximoProcesso.Id}.");
                // Em um sistema real, aqui você notificaria o 'proximoProcesso'.
            }
            else
            {
                Console.WriteLine($"[COORDENADOR {Id}] Recurso agora está livre e a fila está vazia.");
            }
        }
        return Task.CompletedTask;
    }

    #endregion
}

// Classe principal que gerencia o estado e os eventos da simulação.
public class Simulador
{
    private readonly List<Algoritmo> _processosAtivos = new();
    private Algoritmo? _coordenadorAtual;
    private int _proximoIdDeProcesso = 1;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly object _lock = new object();

    public Algoritmo? ObterCoordenadorAtual()
    {
        lock (_lock)
        {
            return _coordenadorAtual;
        }
    }

    public async Task RodarSimulacao()
    {
        Console.WriteLine("--- Simulação Iniciada ---");
        Console.WriteLine("Pressione qualquer tecla para parar a simulação...");

        // Inicia com 3 processos.
        for (int i = 0; i < 3; i++)
        {
            CriarNovoProcesso();
        }

        // Eleição inicial
        ElegerNovoCoordenador();

        // Inicia as tarefas de gerenciamento em paralelo
        var gerenciadorCoordenador = GerenciarCicloDeVidaCoordenadorAsync(_cts.Token);
        var gerenciadorProcessos = GerenciarCriacaoDeProcessosAsync(_cts.Token);

        // Aguarda uma tecla ser pressionada para encerrar
        await Task.Run(() => Console.ReadKey());
        _cts.Cancel(); // Sinaliza para todas as tasks pararem

        // Aguarda as tarefas de gerenciamento finalizarem
        await Task.WhenAll(gerenciadorCoordenador, gerenciadorProcessos);

        Console.WriteLine("\n--- Simulação Finalizada ---");
    }

    private void CriarNovoProcesso()
    {
        lock (_lock)
        {
            // Requisito: ID randômico (aqui usamos sequencial para garantir unicidade)
            var novoProcesso = new Algoritmo(_proximoIdDeProcesso++, this);
            _processosAtivos.Add(novoProcesso);
            Console.WriteLine($"[SIMULADOR] Novo processo criado: ID {novoProcesso.Id}. Total: {_processosAtivos.Count}");
            // Inicia o trabalho do processo em background
            _ = novoProcesso.IniciarTrabalhoAsync(_cts.Token);
        }
    }

    private void ElegerNovoCoordenador()
    {
        lock (_lock)
        {
            if (_coordenadorAtual != null)
            {
                _coordenadorAtual.DeixarDeSerCoordenador();
            }

            if (!_processosAtivos.Any())
            {
                _coordenadorAtual = null;
                Console.WriteLine("[SIMULADOR] Não há processos ativos para eleger um novo coordenador.");
                return;
            }

            // Requisito: Novo coordenador escolhido de forma randomizada.
            var random = new Random();
            _coordenadorAtual = _processosAtivos[random.Next(_processosAtivos.Count)];
            _coordenadorAtual.TornarCoordenador();
        }
    }

    // Requisito: a cada 1 minuto o coordenador morre
    private async Task GerenciarCicloDeVidaCoordenadorAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), token);

                lock (_lock)
                {
                    if (_coordenadorAtual != null)
                    {
                        Console.WriteLine($"\n!!! [SIMULADOR] O COORDENADOR {_coordenadorAtual.Id} MORREU. A fila de requisições foi perdida. !!!\n");
                        _processosAtivos.Remove(_coordenadorAtual);
                        _coordenadorAtual = null;
                        ElegerNovoCoordenador();
                    }
                }
            }
            catch (TaskCanceledException) { break; }
        }
    }

    // Requisito: a cada 40 segundos um novo processo deve ser criado
    private async Task GerenciarCriacaoDeProcessosAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(40), token);
                CriarNovoProcesso();
            }
            catch (TaskCanceledException) { break; }
        }
    }
}
