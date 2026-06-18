# Documento de Visão: ESTG-ESI-Jogo

### (a) Objetivo
[cite_start]Este tópico define o que o sistema fará[cite: 22]. O nosso projeto tem como objetivo criar uma plataforma de entretenimento social no formato de *party game* multiplayer. O sistema permite a um grupo de pessoas jogar minijogos interativos de forma local, onde a ação principal é processada e exibida num ecrã central (Unity), enquanto cada jogador utiliza o seu próprio *smartphone* como comando sem fios.

### (b) Escopo
[cite_start]Como indicativo de onde o projeto pode ser usado[cite: 23], o sistema foi desenhado para animar eventos sociais presenciais. O seu ambiente ideal inclui festas domésticas, encontros de estudantes, convívios de grupos académicos (como atuações e encontros de Tunas), e eventos de *teambuilding*, onde o foco está na interação rápida e divertida entre os participantes.

### (c) Partes interessadas (Stakeholders)
[cite_start]Esta secção define quem usará e quem se beneficiará do sistema[cite: 24]:
* **Jogadores Casuais:** Utilizadores finais que procuram diversão imediata e sem atrito tecnológico (sem necessidade de instalar aplicações nativas).
* **Anfitriões/Hosts de Eventos:** Pessoas que necessitam de ferramentas interativas simples e dinâmicas para animar um grupo.
* **Equipa de Desenvolvimento:** Beneficiários diretos para efeitos de avaliação na disciplina de Engenharia de Software I.
* **Professores e Avaliadores:** Que irão testar o sistema como prova de conceito.

### (d) Equipe do projeto
[cite_start]O nosso Time de Desenvolvimento [cite: 25] dividiu o projeto da seguinte forma:
* **Hugo Marques:** Integração no Motor de Jogo e UI Core (Unity / C#).
* **Miguel Colaço:** Backend, Redes e Gestão de Sessões (Node.js / Socket.IO).
* **Filipe:** Frontend Web e Interface de Comandos Mobile (HTML / CSS / JS).
* **Miguel Fernandes e André:** Direção de Arte, Design de Interfaces e UX.

### (e) Características do sistema
[cite_start]A nossa lista de Funcionalidades [cite: 27] [cite_start]e características centrais do sistema [cite: 26] foca-se na estabilidade e rapidez:
1. **Geração de Salas Virtuais:** Criação dinâmica de instâncias de jogo com um código único, permitindo várias sessões em simultâneo.
2. [cite_start]**Entrada Web (Frictionless):** Sendo uma aplicação WEB[cite: 8], os jogadores acedem ao comando apenas introduzindo o código no *browser* do telemóvel.
3. **Comunicação Bidirecional em Tempo Real:** Sincronização via WebSockets para garantir latência mínima entre os telemóveis e o ecrã principal.
4. **Interfaces Dinâmicas e Responsivas:** A UI mobile altera-se autonomamente consoante o estado do jogo (ex: alternando entre teclados para escrever e botões para votar).
5. **Máquina de Estados Interna:** Gestão de temporizadores, cálculo de pontuações, atribuição de bónus e geração de *Leaderboards*.
6. [cite_start]**Base de Dados Embutida:** Utilização de estruturas de dados e memória de sessão para gerir o estado dos jogadores, cumprindo o requisito de ser uma aplicação que pode ser executada no localhost[cite: 8].

### (f) Arquitetura de Referência
[cite_start]O desenho indicando os componentes do sistema [cite: 29] [cite_start]baseia-se numa Arquitetura de Referência [cite: 28] Cliente-Servidor dividida em três nós:
* **Cliente Host (Unity):** Atua como a "Fonte de Verdade" (*Single Source of Truth*), processando as regras dos minijogos e renderizando a interface principal.
* **Servidor Roteador (Node.js):** Atua como o "polícia de trânsito", gerindo os *WebSockets* e o isolamento do tráfego das salas virtuais.
* **Cliente Jogador (Browser Web):** A aplicação *frontend* mobile que envia os *inputs* do utilizador.

### (g) Restrições do produto
[cite_start]As características que levam o sistema a não funcionar [cite: 30, 31] são:
* **Dependência de Rede:** É estritamente necessária uma ligação à internet estável (ou rede local partilhada) para que os telemóveis e o host consigam comunicar com o servidor.
* **Lotação Máxima:** O sistema está mecanicamente bloqueado a um máximo de 8 jogadores por sala para garantir a fluidez da UI no ecrã e prevenir sobrecarga de *broadcasting*.
* **Estado do Servidor:** O sistema falhará se o serviço *backend* em Node.js não estiver ativamente a correr.

### (h) Integração LLM (Opcional)
[cite_start]Como plano de expansão e funcionalidade apoiada por um LLM [cite: 9][cite_start], pretendemos usar o LLM [cite: 32] [cite_start](como o Ollama localmente [cite: 9]) para a geração processual de conteúdo. O LLM funcionaria como um "Mestre de Jogo", analisando as respostas anteriores dos jogadores para gerar automaticamente novas frases e cenários incompletos para o minijogo "Split the Room", tornando a rejogabilidade virtualmente infinita e sempre adaptada ao humor do grupo.
