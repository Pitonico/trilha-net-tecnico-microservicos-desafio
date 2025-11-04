# trilha-net-tecnico-microservicos-desafio

🧩 Desafio DIO — Microsserviços .NET

Para utilizar com Docker, certifique-se de ter o Docker Desktop instalado:
👉 https://www.docker.com/products/docker-desktop/

No diretório raiz do projeto, execute:

**docker-compose -f docker-compose.prod.yml up --build -d**

Após o build e a inicialização dos containers, acesse os serviços pelos links abaixo:

Serviço URL Descrição

- 🧭 Gateway: http://localhost:5000
- 📦 Estoque: http://localhost:5001
- 💰 Vendas: http://localhost:5002
- 🐇 RabbitMQ UI: http://localhost:15672

🧹 Para parar e remover os containers

👉 **docker-compose -f docker-compose.prod.yml down**

💡 Se quiser visualizar os logs em tempo real:

👉 **docker-compose -f docker-compose.prod.yml logs -f**
