# Testes e Validação do Sistema

De forma a garantir a estabilidade do nosso *party game*, executámos testes em duas frentes: testes à lógica de código (Unitários) e testes funcionais baseados nas User Stories (Aceitação).

## 1. Testes Unitários e de Integração
* **Backend (Node.js):** Testámos a função de geração de código de sala (`generateRoomCode()`) para garantir que cria sempre uma *string* alfanumérica de 4 caracteres. Validámos também a restrição de lotação, confirmando que o servidor rejeita a 9ª ligação a uma mesma sala.
* **Motor de Jogo (Unity):** Validámos a lógica matemática do `SplitRoomManager`, garantindo que o cálculo de pontuações (diferença de votos e bónus de tempo) é feito corretamente antes de atualizar o ecrã de classificação.

## 2. Testes de Aceitação (Baseados nas User Stories)
Executámos cenários práticos para validar as User Stories definidas na fase de planeamento:

* **US01 e US02 (Host - Criação e Lobby):** O Host inicia o jogo no Unity. O sistema gera a sala e mostra o código no ecrã. O teste passou com sucesso, não havendo conflitos com salas antigas.
* **US04 (Jogador - Entrada Fluida):** Tentámos aceder via *browser* mobile (iOS e Android), inserir um nome, escolher um avatar e submeter. O telemóvel conectou-se ao servidor e o avatar apareceu no ecrã do Unity em menos de 1 segundo. Teste de aceitação validado.
* **US05 (Jogador - Sincronização):** O Host avançou da fase de Lobby para a Fase de Escrita. O telemóvel atualizou instantaneamente a sua interface (escondendo os avatares e mostrando o campo de texto), confirmando o sucesso da comunicação *Broadcast* via WebSockets.
