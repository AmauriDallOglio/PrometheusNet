using Prometheus;
using PrometheusNet.Api.Core.Dominio.Entidade;
using PrometheusNet.Api.Core.Infra.Repositorio;

namespace PrometheusNet.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Adicionar serviços ao contêiner
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Registro do repositório de mensagens
            builder.Services.AddSingleton<MensagemRepositorio>();

            var app = builder.Build();

            // Configurações de ambiente de desenvolvimento
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // Middleware de métricas Prometheus
            app.UseRouting();
            app.UseHttpMetrics(); // Métricas automáticas de requisições HTTP
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics(); // Expor as métricas em /metrics
            });

            // Teste inicial
            app.MapGet("/", () => "API de Mensagens com Prometheus!");

            // Métricas personalizadas
            var mensagemCounter = Metrics.CreateCounter("mensagens_cadastradas_total", "Total de mensagens cadastradas.");
            var mensagemGauge = Metrics.CreateGauge("mensagens_ativas", "Número de mensagens atualmente no repositório.");

            // Endpoint para cadastrar mensagens
            app.MapPost("/mensagens", (Mensagem mensagem, MensagemRepositorio repo) =>
            {
                mensagem.Id = Guid.NewGuid();
                mensagem.DataEnvio = DateTime.UtcNow;

                repo.Adicionar(mensagem);

                // Atualizar métricas Prometheus
                mensagemCounter.Inc(); // Incrementa o contador
                mensagemGauge.Set(repo.Listar().Count()); // Atualiza o gauge

                return Results.Ok(mensagem);
            });

            app.Run();
        }
    }
}
