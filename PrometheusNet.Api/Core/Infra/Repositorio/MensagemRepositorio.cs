using PrometheusNet.Api.Core.Dominio.Entidade;
using System.Collections.Concurrent;

namespace PrometheusNet.Api.Core.Infra.Repositorio
{
    public class MensagemRepositorio
    {
        private readonly ConcurrentBag<Mensagem> _mensagens = new ConcurrentBag<Mensagem>();

        public void Adicionar(Mensagem mensagem)
        {
            _mensagens.Add(mensagem);
        }

        public IEnumerable<Mensagem> Listar()
        {
            return _mensagens;
        }
    }
}
