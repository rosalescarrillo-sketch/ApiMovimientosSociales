using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiMovimientosSociales.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrosController : ControllerBase
    {
        private readonly string _connectionString;

        public RegistrosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        [HttpGet("listar")]
        public IActionResult ListarRegistros([FromQuery] string? rol, [FromQuery] string? departamento, [FromQuery] string? distrito)
        {
            try
            {
                var lista = new List<object>();

                using SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();

                string query = @"SELECT Id, Nombres, Apellidos, DUI, Telefono, Cargo, Comunidad, Departamento, Distrito
                                 FROM Registros";

                if (!string.IsNullOrWhiteSpace(rol) &&
                    rol.ToLower() == "departamental" &&
                    !string.IsNullOrWhiteSpace(departamento))
                {
                    query += " WHERE LOWER(Departamento) = LOWER(@departamento)";
                }
                else if (!string.IsNullOrWhiteSpace(rol) &&
                         rol.ToLower() == "distrital" &&
                         !string.IsNullOrWhiteSpace(distrito))
                {
                    query += " WHERE LOWER(Distrito) = LOWER(@distrito)";
                }

                query += " ORDER BY Id DESC";

                using SqlCommand cmd = new SqlCommand(query, conn);

                if (!string.IsNullOrWhiteSpace(rol) &&
                    rol.ToLower() == "departamental" &&
                    !string.IsNullOrWhiteSpace(departamento))
                {
                    cmd.Parameters.AddWithValue("@departamento", departamento);
                }
                else if (!string.IsNullOrWhiteSpace(rol) &&
                         rol.ToLower() == "distrital" &&
                         !string.IsNullOrWhiteSpace(distrito))
                {
                    cmd.Parameters.AddWithValue("@distrito", distrito);
                }

                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new
                    {
                        id = Convert.ToInt32(reader["Id"]),
                        nombres = reader["Nombres"]?.ToString() ?? "",
                        apellidos = reader["Apellidos"]?.ToString() ?? "",
                        dui = reader["DUI"]?.ToString() ?? "",
                        telefono = reader["Telefono"]?.ToString() ?? "",
                        cargo = reader["Cargo"]?.ToString() ?? "",
                        comunidad = reader["Comunidad"]?.ToString() ?? "",
                        departamento = reader["Departamento"]?.ToString() ?? "",
                        distrito = reader["Distrito"]?.ToString() ?? ""
                    });
                }

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al listar registros", error = ex.Message });
            }
        }

        [HttpPost("crear")]
        public IActionResult CrearRegistro([FromBody] Registro nuevo)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();

                string query = @"INSERT INTO Registros
                                (Nombres, Apellidos, DUI, Telefono, Cargo, Comunidad, Departamento, Distrito)
                                VALUES
                                (@nombres, @apellidos, @dui, @telefono, @cargo, @comunidad, @departamento, @distrito)";

                using SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@nombres", nuevo.nombres ?? "");
                cmd.Parameters.AddWithValue("@apellidos", nuevo.apellidos ?? "");
                cmd.Parameters.AddWithValue("@dui", nuevo.dui ?? "");
                cmd.Parameters.AddWithValue("@telefono", nuevo.telefono ?? "");
                cmd.Parameters.AddWithValue("@cargo", nuevo.cargo ?? "");
                cmd.Parameters.AddWithValue("@comunidad", nuevo.comunidad ?? "");
                cmd.Parameters.AddWithValue("@departamento", nuevo.departamento ?? "");
                cmd.Parameters.AddWithValue("@distrito", nuevo.distrito ?? "");

                cmd.ExecuteNonQuery();

                return Ok(new { mensaje = "Registro creado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear registro", error = ex.Message });
            }
        }

        [HttpPut("editar/{id}")]
        public IActionResult EditarRegistro(int id, [FromBody] Registro datos)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();

                string query = @"UPDATE Registros
                                 SET Nombres = @nombres,
                                     Apellidos = @apellidos,
                                     DUI = @dui,
                                     Telefono = @telefono,
                                     Cargo = @cargo,
                                     Comunidad = @comunidad,
                                     Departamento = @departamento,
                                     Distrito = @distrito
                                 WHERE Id = @id";

                using SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@nombres", datos.nombres ?? "");
                cmd.Parameters.AddWithValue("@apellidos", datos.apellidos ?? "");
                cmd.Parameters.AddWithValue("@dui", datos.dui ?? "");
                cmd.Parameters.AddWithValue("@telefono", datos.telefono ?? "");
                cmd.Parameters.AddWithValue("@cargo", datos.cargo ?? "");
                cmd.Parameters.AddWithValue("@comunidad", datos.comunidad ?? "");
                cmd.Parameters.AddWithValue("@departamento", datos.departamento ?? "");
                cmd.Parameters.AddWithValue("@distrito", datos.distrito ?? "");

                int filas = cmd.ExecuteNonQuery();

                if (filas > 0)
                    return Ok(new { mensaje = "Registro actualizado correctamente" });

                return NotFound(new { mensaje = "Registro no encontrado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al editar registro", error = ex.Message });
            }
        }

        [HttpDelete("eliminar/{id}")]
        public IActionResult EliminarRegistro(int id)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();

                string query = "DELETE FROM Registros WHERE Id = @id";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                int filas = cmd.ExecuteNonQuery();

                if (filas > 0)
                    return Ok(new { mensaje = "Registro eliminado correctamente" });

                return NotFound(new { mensaje = "Registro no encontrado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar registro", error = ex.Message });
            }
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();

                int totalRegistros;
                int totalDepartamentos;
                int totalDistritos;

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Registros", conn))
                    totalRegistros = Convert.ToInt32(cmd.ExecuteScalar());

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(DISTINCT Departamento) FROM Registros", conn))
                    totalDepartamentos = Convert.ToInt32(cmd.ExecuteScalar());

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(DISTINCT Distrito) FROM Registros", conn))
                    totalDistritos = Convert.ToInt32(cmd.ExecuteScalar());

                var porDepartamento = new List<object>();
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT Departamento, COUNT(*) AS total
                    FROM Registros
                    GROUP BY Departamento
                    ORDER BY total DESC", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        porDepartamento.Add(new
                        {
                            departamento = reader["Departamento"]?.ToString() ?? "",
                            total = Convert.ToInt32(reader["total"])
                        });
                    }
                }

                var topDistritos = new List<object>();
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT TOP 10 Distrito, COUNT(*) AS total
                    FROM Registros
                    GROUP BY Distrito
                    ORDER BY total DESC, Distrito ASC", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        topDistritos.Add(new
                        {
                            distrito = reader["Distrito"]?.ToString() ?? "",
                            total = Convert.ToInt32(reader["total"])
                        });
                    }
                }

                var distritosBajos = new List<object>();
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT TOP 3 Distrito, COUNT(*) AS total
                    FROM Registros
                    GROUP BY Distrito
                    ORDER BY total ASC, Distrito ASC", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        distritosBajos.Add(new
                        {
                            distrito = reader["Distrito"]?.ToString() ?? "",
                            total = Convert.ToInt32(reader["total"])
                        });
                    }
                }

                return Ok(new
                {
                    totalRegistros,
                    totalDepartamentos,
                    totalDistritos,
                    porDepartamento,
                    topDistritos,
                    distritosBajos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al cargar dashboard", error = ex.Message });
            }
        }

        [HttpGet("exportar-excel")]
        public IActionResult ExportarExcel()
        {
            try
            {
                var lista = new List<Registro>();

                using SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();

                string query = @"SELECT Id, Nombres, Apellidos, DUI, Telefono, Cargo, Comunidad, Departamento, Distrito
                                 FROM Registros
                                 ORDER BY Id DESC";

                using SqlCommand cmd = new SqlCommand(query, conn);
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Registro
                    {
                        id = Convert.ToInt32(reader["Id"]),
                        nombres = reader["Nombres"]?.ToString() ?? "",
                        apellidos = reader["Apellidos"]?.ToString() ?? "",
                        dui = reader["DUI"]?.ToString() ?? "",
                        telefono = reader["Telefono"]?.ToString() ?? "",
                        cargo = reader["Cargo"]?.ToString() ?? "",
                        comunidad = reader["Comunidad"]?.ToString() ?? "",
                        departamento = reader["Departamento"]?.ToString() ?? "",
                        distrito = reader["Distrito"]?.ToString() ?? ""
                    });
                }

                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Registros");

                ws.Cell(1, 1).Value = "ID";
                ws.Cell(1, 2).Value = "Nombres";
                ws.Cell(1, 3).Value = "Apellidos";
                ws.Cell(1, 4).Value = "DUI";
                ws.Cell(1, 5).Value = "Teléfono";
                ws.Cell(1, 6).Value = "Cargo";
                ws.Cell(1, 7).Value = "Comunidad";
                ws.Cell(1, 8).Value = "Departamento";
                ws.Cell(1, 9).Value = "Distrito";

                int fila = 2;
                foreach (var item in lista)
                {
                    ws.Cell(fila, 1).Value = item.id;
                    ws.Cell(fila, 2).Value = item.nombres;
                    ws.Cell(fila, 3).Value = item.apellidos;
                    ws.Cell(fila, 4).Value = item.dui;
                    ws.Cell(fila, 5).Value = item.telefono;
                    ws.Cell(fila, 6).Value = item.cargo;
                    ws.Cell(fila, 7).Value = item.comunidad;
                    ws.Cell(fila, 8).Value = item.departamento;
                    ws.Cell(fila, 9).Value = item.distrito;
                    fila++;
                }

                ws.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "registros.xlsx"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al exportar Excel", error = ex.Message });
            }
        }
    }
}