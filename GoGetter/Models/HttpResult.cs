namespace GoGetter.Models;

public class HttpResult<T>(T value, string message, int httpCode = 999)
{
	public T Value => value;
	public int HttpCode => httpCode;
	public string Message => message;
	public bool IsSuccess => (httpCode < 300);
}
