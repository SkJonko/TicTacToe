using System.Text.Json.Serialization;
using TicToe.Infinite.Data.Models;

[JsonSerializable(typeof(Game))]
public partial class JsonContext : JsonSerializerContext
{
}