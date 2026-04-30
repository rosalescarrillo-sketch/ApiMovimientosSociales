using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiMovimientosSociales.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly string _connectionString;

        public UsuariosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        [HttpPost("crear")]
        public IActionResult CrearUsuario([FromBody] UsuarioRegistro nuevoUsuario)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();

                string verificar = "SELECT COUNT(*) FROM usuarios WHERE usuario = @usuario";
                using SqlCommand cmdVerificar = new SqlCommand(verificar, conn);
                cmdVerificar.Parameters.AddWithValue("@usuario", nuevoUsuario.usuario ?? "");

                int existe = (int)cmdVerificar.ExecuteScalar();

                if (existe > 0)
                {
                    return BadRequest(new { mensaje = "El usuario ya existe" });
                }

                string query = @"INSERT INTO usuarios (usuario, password, rol, departamento, distrito)
                                 VALUES (@usuario, @password, @rol, @departamento, @distrito)";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@usuario", nuevoUsuario.usuario ?? "");
                cmd.Parameters.AddWithValue("@password", nuevoUsuario.password ?? "");
                cmd.Parameters.AddWithValue("@rol", nuevoUsuario.rol ?? "");
                cmd.Parameters.AddWithValue("@departamento", nuevoUsuario.departamento ?? "");
                cmd.Parameters.AddWithValue("@distrito", nuevoUsuario.distrito ?? "");

                cmd.ExecuteNonQuery();

                return Ok(new { mensaje = "Usuario creado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al crear usuario",
                    error = ex.Message
                });
            }
        }

        [HttpGet("listar")]
        public IActionResult ListarUsuarios()
        {
            try
            {
                List<object> lista = new List<object>();

                using SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();

                string query = "SELECT id, usuario, rol, departamento, distrito FROM usuarios ORDER BY id DESC";

                using SqlCommand cmd = new SqlCommand(query, conn);
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new
                    {
                        id = Convert.ToInt32(reader["id"]),
                        usuario = reader["usuario"]?.ToString() ?? "",
                        rol = reader["rol"]?.ToString() ?? "",
                        departamento = reader["departamento"]?.ToString() ?? "",
                        distrito = reader["distrito"]?.ToString() ?? ""
                    });
                }

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al listar usuarios",
                    error = ex.Message
                });
            }
        }
        [HttpPut("cambiar-password/{id}")]
        public IActionResult CambiarPassword(int id, [FromBody] UsuarioRegistro datos)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(datos.password))
                {
                    return BadRequest(new { mensaje = "Debes escribir una nueva contraseña" });
                }

                using SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();

                string query = "UPDATE usuarios SET password = @password WHERE id = @id";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@password", datos.password);
                cmd.Parameters.AddWithValue("@id", id);

                int filas = cmd.ExecuteNonQuery();

                if (filas > 0)
                {
                    return Ok(new { mensaje = "Contraseña actualizada correctamente" });
                }

                return NotFound(new { mensaje = "Usuario no encontrado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al cambiar contraseña",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("eliminar/{id}")]
        public IActionResult EliminarUsuario(int id)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();

                // 1. Verificar rol del usuario
                string validar = "SELECT rol FROM usuarios WHERE id = @id";
                using SqlCommand cmdVal = new SqlCommand(validar, conn);
                cmdVal.Parameters.AddWithValue("@id", id);

                var rol = cmdVal.ExecuteScalar()?.ToString();

                if ((rol ?? "").ToLower() == "admin")
                {
                    return BadRequest(new { mensaje = "No se puede eliminar un usuario administrador" });
                }

                // 2. Eliminar si no es admin
                string query = "DELETE FROM usuarios WHERE id = @id";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                int filas = cmd.ExecuteNonQuery();

                if (filas > 0)
                {
                    return Ok(new { mensaje = "Usuario eliminado correctamente" });
                }

                return NotFound(new { mensaje = "Usuario no encontrado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al eliminar usuario",
                    error = ex.Message
                });
            }
        }
    }
}