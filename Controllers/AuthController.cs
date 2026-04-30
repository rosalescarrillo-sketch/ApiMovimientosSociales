using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiMovimientosSociales.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly string _connectionString;

        public AuthController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();

                string query = @"SELECT TOP 1 id, usuario, password, rol, departamento, distrito
                                 FROM usuarios
                                 WHERE usuario = @usuario AND password = @password";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@usuario", request.usuario ?? "");
                cmd.Parameters.AddWithValue("@password", request.password ?? "");

                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return Ok(new
                    {
                        id = Convert.ToInt32(reader["id"]),
                        usuario = reader["usuario"]?.ToString() ?? "",
                        rol = reader["rol"]?.ToString() ?? "",
                        departamento = reader["departamento"]?.ToString() ?? "",
                        distrito = reader["distrito"]?.ToString() ?? ""
                    });
                }

                return Unauthorized(new { mensaje = "Credenciales incorrectas" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al conectar con la base de datos",
                    error = ex.Message
                });
            }
        }
    }
}