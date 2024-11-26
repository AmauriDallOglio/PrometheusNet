namespace PrometheusNet.Api.Core.Dominio.Entidade
{
    public class Mensagem
    {
        public Guid Id { get; set; }
        public string Texto { get; set; }
        public DateTime DataEnvio { get; set; }
    }
}
