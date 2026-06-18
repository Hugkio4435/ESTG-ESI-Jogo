# Documento de Retrospectiva e Relatório Final

## 1. Resumo do Projeto
O desenvolvimento do "ESTG-ESI-Jogo" foi concluído com sucesso, resultando num *party game* multiplayer funcional, interativo e divertido. A equipa conseguiu cumprir o objetivo principal (Apesar de só termos feito 2 mini-jogos, e não 4) : interligar três ecossistemas tecnológicos distintos (Unity/C#, Node.js e HTML/JS) numa única experiência em tempo real. 

## 2. Organização e Metodologia
A nossa equipa dividiu o trabalho recorrendo a princípios ágeis de desenvolvimento. A separação de responsabilidades foi clara desde o início:
* **Frontend Mobile:** Focado na responsividade e na experiência do utilizador no telemóvel.
* **Backend:** Focado na estabilidade da rede, roteamento de mensagens e segurança das salas.
* **Motor de Jogo:** Focado na máquina de estados, lógica de minijogos e renderização visual.
* **Design UI/UX:** Focado em garantir paletas de alto contraste e interfaces intuitivas criadas em Figma.

## 3. Desafios Encontrados e Soluções
Durante o ciclo de desenvolvimento, enfrentámos desafios técnicos significativos:
* **Latência e Sincronização:** Inicialmente, gerir a passagem de eventos entre o Unity e o Node.js levantou questões de dessincronização. Resolvemos isto implementando filas de ações (Queues) no Unity para garantir que a interface principal só era atualizada na *Main Thread*, evitando *crashes*.
* **Gestão de Versões (Git):** Tivemos desafios no controlo de versões, nomeadamente com bloqueios de ficheiros gerados pelo Visual Studio (pasta `.vs`) que impediam os *commits*. Solucionámos o problema atualizando o `.gitignore`, o que nos ensinou a importância de manter repositórios limpos de ficheiros locais.
* **Responsividade do "Comando":** Garantir que a interface HTML ficava perfeita em qualquer telemóvel exigiu afinações rigorosas no CSS para impedir que teclados virtuais desfigurassem os ecrãs de votação.

## 4. Conclusão
O projeto permitiu consolidar os conhecimentos adquiridos na disciplina de Engenharia de Software I, demonstrando na prática como a Arquitetura de Software e as User Stories são fundamentais para não perder o foco quando se integram várias linguagens de programação no mesmo sistema.
