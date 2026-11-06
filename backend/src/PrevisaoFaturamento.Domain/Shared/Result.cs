namespace PrevisaoFaturamento.Domain.Shared;

public enum TipoErro
{
    Nenhum = 0,
    Validacao = 400,      // 400 Bad Request (Regras de negócio / entrada inválida)
    NaoAutorizado = 401,  // 401 Unauthorized (Credenciais inválidas)
    Proibido = 403,       // 403 Forbidden
    NaoEncontrado = 404,  // 404 Not Found
    Conflito = 409        // 409 Conflict (Ex: E-mail ou CNPJ duplicado)
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; }
    public TipoErro TipoErro { get; }

    protected Result(bool isSuccess, string? errorMessage, TipoErro tipoErro)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        TipoErro = tipoErro;
    }

    public static Result Success() => new(true, null, TipoErro.Nenhum);
    
    public static Result Failure(string errorMessage, TipoErro tipoErro = TipoErro.Validacao) 
        => new(false, errorMessage, tipoErro);
}

public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool isSuccess, T? data, string? errorMessage, TipoErro tipoErro)
        : base(isSuccess, errorMessage, tipoErro)
    {
        Data = data;
    }

    public static Result<T> Success(T data) 
        => new(true, data, null, TipoErro.Nenhum);

    public new static Result<T> Failure(string errorMessage, TipoErro tipoErro = TipoErro.Validacao) 
        => new(false, default, errorMessage, tipoErro);
}