namespace TicToe.Infinite.Services;

/// <summary>
/// Error Handler Service.
/// </summary>
public interface IErrorHandler
{
	/// <summary>
	/// Handle error in UI.
	/// </summary>
	/// <param name="ex">Exception being thrown.</param>
	public void HandleError(Exception ex);
}