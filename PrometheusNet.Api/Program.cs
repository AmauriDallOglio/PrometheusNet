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

            var app = builder.Build();

            // Configura��es de ambiente de desenvolvimento
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // Middleware de m�tricas Prometheus
            app.UseRouting();
            app.UseHttpMetrics(); // M�tricas autom�ticas de requisi��es HTTP
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics(); // Expor as m�tricas em /metrics
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
                mensagem.DataEnvio = DateTime.UtcNow;

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
