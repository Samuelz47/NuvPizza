namespace NuvPizza.Application.Common;

public class Result<T> 
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; }
    public T? Data { get; private set; }

    // Construtor privado para forçar o uso dos métodos estáticos
    private Result() { }

    public static Result<T> Success(T data) 
    {
        return new Result<T> { IsSuccess = true, Data = data };
    }

    public static Result<T> Failure(string message) 
    {
        return new Result<T> { IsSuccess = false, Message = message };
    }
}