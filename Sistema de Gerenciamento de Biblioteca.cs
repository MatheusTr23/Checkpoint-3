using System;
using System.Collections.Generic;

// Interface de Notificação
public interface INotificador
{
    void Enviar(string destinatario, string mensagem);
}

// Implementação de Notificação via Email
public class EmailNotificador : INotificador
{
    public void Enviar(string destinatario, string mensagem)
    {
        Console.WriteLine($"Email enviado para {destinatario}: {mensagem}");
    }
}

// Implementação de Notificação via SMS
public class SMSNotificador : INotificador
{
    public void Enviar(string destinatario, string mensagem)
    {
        Console.WriteLine($"SMS enviado para {destinatario}: {mensagem}");
    }
}

// Classe Livro
public class Livro
{
    public string Titulo { get; set; }
    public string Autor { get; set; }
    public string ISBN { get; set; }
    public bool Disponivel { get; set; } = true;
}

// Classe Usuário
public class Usuario
{
    public string Nome { get; set; }
    public int ID { get; set; }
}

// Interface para cálculo de multas
public interface ICalculoMulta
{
    double CalcularMulta(DateTime dataDevolucaoPrevista, DateTime dataDevolucaoEfetiva);
}

// Implementação padrão da multa (R$ 1,00 por dia de atraso)
public class CalculoMultaPadrao : ICalculoMulta
{
    public double CalcularMulta(DateTime dataDevolucaoPrevista, DateTime dataDevolucaoEfetiva)
    {
        return (dataDevolucaoEfetiva > dataDevolucaoPrevista) ? 
            (dataDevolucaoEfetiva - dataDevolucaoPrevista).Days * 1.0 : 0;
    }
}

// Classe Empréstimo
public class Emprestimo
{
    public Livro Livro { get; set; }
    public Usuario Usuario { get; set; }
    public DateTime DataEmprestimo { get; set; }
    public DateTime DataDevolucaoPrevista { get; set; }
    public DateTime? DataDevolucaoEfetiva { get; set; }

    private readonly ICalculoMulta _calculoMulta;

    public Emprestimo(ICalculoMulta calculoMulta)
    {
        _calculoMulta = calculoMulta;
    }

    public double CalcularMulta()
    {
        return DataDevolucaoEfetiva.HasValue 
            ? _calculoMulta.CalcularMulta(DataDevolucaoPrevista, DataDevolucaoEfetiva.Value) 
            : 0;
    }
}

// Classe Gerenciador de Biblioteca
public class Biblioteca
{
    private readonly List<Livro> livros = new List<Livro>();
    private readonly List<Usuario> usuarios = new List<Usuario>();
    private readonly List<Emprestimo> emprestimos = new List<Emprestimo>();
    private readonly INotificador _notificador;

    public Biblioteca(INotificador notificador)
    {
        _notificador = notificador;
    }

    public void AdicionarLivro(string titulo, string autor, string isbn)
    {
        livros.Add(new Livro { Titulo = titulo, Autor = autor, ISBN = isbn });
        Console.WriteLine($"Livro '{titulo}' adicionado!");
    }

    public void AdicionarUsuario(string nome, int id)
    {
        usuarios.Add(new Usuario { Nome = nome, ID = id });
        Console.WriteLine($"Usuário '{nome}' cadastrado!");
        _notificador.Enviar(nome, "Bem-vindo à Biblioteca!");
    }

    public bool RealizarEmprestimo(int usuarioId, string isbn, int diasEmprestimo)
    {
        var livro = livros.Find(l => l.ISBN == isbn);
        var usuario = usuarios.Find(u => u.ID == usuarioId);

        if (livro == null || usuario == null || !livro.Disponivel) return false;

        livro.Disponivel = false;
        var emprestimo = new Emprestimo(new CalculoMultaPadrao())
        {
            Livro = livro,
            Usuario = usuario,
            DataEmprestimo = DateTime.Now,
            DataDevolucaoPrevista = DateTime.Now.AddDays(diasEmprestimo)
        };
        emprestimos.Add(emprestimo);

        _notificador.Enviar(usuario.Nome, $"Você pegou emprestado o livro '{livro.Titulo}'!");
        return true;
    }

    public double RealizarDevolucao(string isbn, int usuarioId)
    {
        var emprestimo = emprestimos.Find(e => e.Livro.ISBN == isbn && e.Usuario.ID == usuarioId);
        if (emprestimo == null) return -1;

        emprestimo.DataDevolucaoEfetiva = DateTime.Now;
        emprestimo.Livro.Disponivel = true;
        double multa = emprestimo.CalcularMulta();

        _notificador.Enviar(emprestimo.Usuario.Nome, $"Multa de R$ {multa} pelo atraso!");
        return multa;
    }
}
