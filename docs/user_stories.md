# User Stories e Testes de Aceitação

Para garantir que o sistema cumpre as necessidades dos utilizadores, dividimos os requisitos em User Stories, separadas por tipo de utilizador (Host/Ecrã Principal e Jogador/Mobile).

### 📺 Utilizador: Host (Ecrã Principal / Unity)

**US01: Criação de Sessão Dinâmica**
* **Como** Host da partida,
* **Eu quero** que o sistema crie uma sala virtual única e exiba um código de acesso legível no ecrã principal,
* **Para que** os jogadores saibam exatamente o código que devem inserir nos seus telemóveis para entrar.

**US02: Gestão Visual do Lobby**
* **Como** Host da partida,
* **Eu quero** ver a grelha de jogadores atualizar-se em tempo real com o nome e avatar de cada pessoa que se liga,
* **Para que** eu consiga confirmar visualmente quem já está pronto antes de iniciar o minijogo.

**US03: Automatização da Leaderboard**
* **Como** Host da partida,
* **Eu quero** que o sistema calcule as pontuações e ordene os jogadores de forma decrescente no final de cada ronda,
* **Para que** o grupo consiga ver claramente quem está a ganhar a competição através de um gráfico limpo e direto.

### 📱 Utilizador: Jogador (Comando Mobile / HTML)

**US04: Entrada Fluida e Segura**
* **Como** jogador mobile,
* **Eu quero** introduzir o código da sala, o meu nome e o meu avatar numa interface web,
* **Para que** consiga entrar na partida a partir de qualquer *smartphone* sem ter de instalar nenhuma aplicação.

**US05: Sincronização de Interfaces em Tempo Real**
* **Como** jogador mobile,
* **Eu quero** que o ecrã do meu telemóvel mude automaticamente (ex: de teclado para botões de votação) assim que a fase do jogo avançar no ecrã principal,
* **Para que** saiba exatamente o que tenho de fazer em cada momento, sem ter de recarregar a página.
