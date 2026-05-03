using OpenTelemetry.Metrics;
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

            // Adicionar serviços ao cont�iner
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Registro do repositorio de mensagens
            builder.Services.AddSingleton<MensagemRepositorio>();



            builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation()
                           .AddHttpClientInstrumentation()
                           .AddPrometheusExporter();
                });

 


            var app = builder.Build();

            // Configuraçoes de ambiente de desenvolvimento
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // Middleware de metricas Prometheus
            app.UseRouting();
            app.UseHttpMetrics(); // coleta automatica de requisiçoes
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics(); // expoes /metrics
            });




            // Teste inicial
            app.MapGet("/", () => "API de Mensagens com Prometheus!");

            // Metricas personalizadas
            var mensagemCounter = Metrics.CreateCounter("mensagens_cadastradas_total", "Total de mensagens cadastradas.");
            var mensagemGauge = Metrics.CreateGauge("mensagens_ativas", "Numero de mensagens atualmente no reposit�rio.");

            // Endpoint para cadastrar mensagens
            app.MapPost("/mensagens", (Mensagem mensagem, MensagemRepositorio repo) =>
            {
                mensagem.Id = Guid.NewGuid();
                mensagem.DataEnvio = DateTime.Now;

                repo.Adicionar(mensagem);

                // Atualizar metricas Prometheus
                mensagemCounter.Inc(); // Incrementa o contador
                mensagemGauge.Set(repo.Listar().Count()); // Atualiza o gauge

                return Results.Ok(mensagem);
            });


            // Rota que simula erro 400 (Bad Request)
            app.MapGet("/erro400", () =>
            {
                return Results.BadRequest(new { mensagem = "Requisição inválida" });
            });

            // Rota que simula erro 401 (Unauthorized)
            app.MapGet("/erro401", () =>
            {
                return Results.Unauthorized();
            });

            // Rota que simula erro 404 (Not Found)
            app.MapGet("/erro404", () =>
            {
                return Results.NotFound(new { mensagem = "Recurso não encontrado" });
            });

            // Rota que simula erro 500 (Internal Server Error)
            app.MapGet("/erro500", () =>
            {
                try
                {
                    throw new Exception("Erro interno simulado");
                }
                catch (Exception ex)
                {
                    return Results.NotFound(new { mensagem = ex.Message });
                }
  
 
            });




            app.Run();
        }
    }
}
