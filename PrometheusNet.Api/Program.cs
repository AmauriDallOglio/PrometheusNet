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

            // Adicionar servi�os ao cont�iner
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Registro do reposit�rio de mensagens
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

            // M�tricas personalizadas
            var mensagemCounter = Metrics.CreateCounter("mensagens_cadastradas_total", "Total de mensagens cadastradas.");
            var mensagemGauge = Metrics.CreateGauge("mensagens_ativas", "N�mero de mensagens atualmente no reposit�rio.");

            // Endpoint para cadastrar mensagens
            app.MapPost("/mensagens", (Mensagem mensagem, MensagemRepositorio repo) =>
            {
                mensagem.Id = Guid.NewGuid();
                mensagem.DataEnvio = DateTime.Now;

                repo.Adicionar(mensagem);

                // Atualizar m�tricas Prometheus
                mensagemCounter.Inc(); // Incrementa o contador
                mensagemGauge.Set(repo.Listar().Count()); // Atualiza o gauge

                return Results.Ok(mensagem);
            });

      
            app.Run();
        }
    }
}
