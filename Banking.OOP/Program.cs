public class ContaBancaria
{
	public int Id { get; set; }
	public string Titular { get; set; }
	public string Documento { get; set; }
	public string Banco { get; set; }
	public string Agencia { get; set; }
	public string Numero { get; set; }
	public decimal LimiteDiario { get; set; }
	public decimal Saldo { get; set; }

	public ICollection<Movimentacao> Movimentacoes { get; set; }
}

public class Movimentacao
{
	public ContaBancaria? Origem { get; set; }
	public ContaBancaria? Destino { get; set; }
	public decimal Valor { get; set; }
	public DateTime Timestamp { get; set; }
}

public class TransferirDinheiroService
{
	public void Transferir(
		ContaBancaria contaOrigem,
		ContaBancaria contaDestino,
		decimal valor,
		DateTime timestamp)
	{
		if (contaOrigem.Banco == contaDestino.Banco &&
			contaOrigem.Agencia == contaDestino.Agencia &&
			contaOrigem.Numero == contaDestino.Numero)
		{
			throw new ArgumentException("Uma transferência só pode ser realizada entre contas diferentes.", nameof(contaDestino));
		}

		if (valor <= 0)
			throw new ArgumentException("Valor deve ser maior que 0.", nameof(valor));

		if (valor > 1000)
			throw new ArgumentException("Valor deve ser menor ou igual a 1000.", nameof(valor));

		var valorTransferidoHoje = contaOrigem.Movimentacoes
											  .Where(t => t.Timestamp.Date == timestamp.Date &&
														  t.Origem.Id == contaOrigem.Id)
											  .Sum(t => t.Valor);

		if (valorTransferidoHoje + valor > contaOrigem.LimiteDiario)
			throw new InvalidOperationException("Limite diário excedido.");

		if (contaOrigem.Saldo < valor)
			throw new InvalidOperationException("Saldo insuficiente.");

		Movimentacao movimentacao = new()
		{
			Origem = contaOrigem,
			Destino = contaDestino,
			Valor = valor,
			Timestamp = timestamp
		};

		contaOrigem.Saldo -= valor;
		contaOrigem.Movimentacoes.Add(movimentacao);

		contaDestino.Saldo += valor;
		contaDestino.Movimentacoes.Add(movimentacao);
	}
}

public class DepositarDinheiroService
{
	public void Depositar(
		ContaBancaria contaDestino,
		decimal valor,
		DateTime timestamp)
	{
		if (valor <= 0)
			throw new ArgumentException("Valor deve ser maior que 0.", nameof(valor));

		Movimentacao movimentacao = new()
		{
			Destino = contaDestino,
			Valor = valor,
			Timestamp = timestamp
		};

		contaDestino.Saldo += valor;
		contaDestino.Movimentacoes.Add(movimentacao);
	}
}

public class SacarDinheiroService
{
	public void Sacar(
		ContaBancaria contaOrigem,
		decimal valor,
		DateTime timestamp)
	{
		if (valor <= 0)
			throw new ArgumentException("Valor deve ser maior que 0.", nameof(valor));

		if (valor > 1000)
			throw new ArgumentException("Valor deve ser menor ou igual a 1000.", nameof(valor));

		if (contaOrigem.Saldo < valor)
			throw new InvalidOperationException("Saldo insuficiente.");

		Movimentacao movimentacao = new()
		{
			Origem = contaOrigem,
			Valor = valor,
			Timestamp = timestamp
		};

		contaOrigem.Saldo -= valor;
		contaOrigem.Movimentacoes.Add(movimentacao);
	}
}