# Documento de Visão: ESTG-ESI-Jogo

### (a) Objetivo
O nosso projeto tem como objetivo desenvolver um sistema no formato de *party game* multiplayer, focado no entretenimento social. Na prática, criámos um jogo que permite a um grupo de pessoas jogar minijogos interativos na mesma sala. A ação principal decorre num ecrã central partilhado (desenvolvido no motor Unity), enquanto cada jogador utiliza o seu próprio telemóvel como comando sem fios.

### (b) Escopo
O nosso sistema foi desenhado para animar eventos sociais presenciais. O ambiente ideal de utilização inclui festas em casa, encontros de estudantes, convívios académicos (como os ensaios e festas da nossa Tuna), e eventos de *teambuilding*. O foco está numa interação rápida, sem atritos e divertida entre a malta.

### (c) Partes interessadas (Stakeholders)
Identificámos os seguintes grupos de pessoas que vão usar ou beneficiar do nosso sistema:
* **Jogadores Casuais:** Quem vai ativamente jogar. Procuram diversão imediata e valorizam não ter de instalar nenhuma aplicação no telemóvel.
* **Anfitriões (Hosts):** A pessoa que organiza o evento e precisa de um jogo fácil de configurar para animar o grupo.
* **A Nossa Equipa:** Como criadores do projeto, beneficiamos diretamente para a nossa avaliação na disciplina de Engenharia de Software I.

### (d) Equipe do projeto
A nossa equipa de desenvolvimento dividiu o trabalho da seguinte forma:
* **Hugo Marques (Eu):** Fiquei responsável pela construção da interface e integração de todo o sistema dentro do Unity (C#).
* **Miguel Colaço:** Desenvolveu a arquitetura de rede e o servidor backend.
* **Filipe:** Construiu o frontend web em HTML que serve de interface no telemóvel.
* **Miguel Fernandes e André:** Trataram da parte criativa e visual, garantindo o design e o UI/UX do jogo.

### (e) Características do sistema
As funcionalidades principais que implementámos no nosso projeto são:
1. **Criação de Salas Virtuais:** O sistema gera automaticamente sessões de jogo com um código único, permitindo isolar os jogadores.
2. **Acesso Web Rápido:** Sendo uma aplicação WEB, os jogadores apenas precisam de introduzir o código no *browser* do telemóvel para começarem a jogar.
3. **Comunicação em Tempo Real:** Utilizamos WebSockets para garantir que não há atrasos na comunicação entre as ações no telemóvel e os resultados na televisão.
4. **Interfaces Adaptativas:** O ecrã do telemóvel altera-se sozinho consoante a fase do jogo (mudando de um teclado para escrever, para botões de votação).
5. **Máquina de Estados e Leaderboards:** O jogo calcula pontuações automaticamente, gere temporizadores e gera as tabelas de classificação finais.
6. **Gestão de Dados em Memória:** Cumprindo os requisitos, o servidor faz a gestão e o mapeamento dos jogadores ativos em memória, podendo ser perfeitamente executado num ambiente *localhost*.

### (f) Arquitetura de Referência
Montámos o nosso sistema utilizando uma arquitetura Cliente-Servidor suportada por três componentes fundamentais:
* **Cliente Host (Unity):** É o cérebro do jogo. Processa as regras, gere as fases do jogo e renderiza todos os gráficos.
* **Servidor Roteador (Node.js):** Atua como o "polícia de trânsito". Gere as ligações ativas e garante que a informação dos telemóveis chega à sala de jogo correta no Unity.
* **Cliente Jogador (Browser Web):** A aplicação *frontend* que envia os *inputs* e votos do utilizador para a rede.

### (g) Restrições do produto
O sistema tem algumas limitações e não funcionará nas seguintes condições:
* **Falta de Rede:** É estritamente obrigatório ter ligação à internet (ou uma rede local bem configurada) para que os telemóveis consigam comunicar com o ecrã principal.
* **Excesso de Jogadores:** Trancámos mecanicamente o limite a 8 jogadores por sala para evitar que o ecrã fique confuso de ler e para garantir a estabilidade do servidor.
* **Servidor Inativo:** O jogo precisa que o nosso serviço *backend* em Node.js esteja a correr continuamente; caso contrário, as ligações caem.

### (h) Integração LLM (Opcional)
Como perspetiva de futuro, planeamos integrar um LLM a correr localmente (como o Ollama). A nossa ideia é colocar o LLM a atuar como um "Mestre de Jogo", analisando as frases e o humor das respostas que os jogadores deram nas rondas anteriores. Com isso, o LLM conseguiria gerar perguntas e cenários completamente novos para o minijogo, tornando a experiência virtualmente infinita e sempre personalizada para aquele grupo específico.
