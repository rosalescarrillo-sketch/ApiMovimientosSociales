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
                List<object> lista = new List<object>();

                using SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();

                string query = @"SELECT id, tipo_registro, tipo_apoyo, nombres, apellidos, dui, telefono, cargo, comunidad, departamento, distrito, fecha, creado_por
                                 FROM registros";

                if (!string.IsNullOrWhiteSpace(rol) && rol.ToLower() == "departamental" && !string.IsNullOrWhiteSpace(departamento))
                {
                    query += " WHERE departamento = @departamento";
                }
                else if (!string.IsNullOrWhiteSpace(rol) && rol.ToLower() == "distrital" && !string.IsNullOrWhiteSpace(distrito))
                {
                    query += " WHERE distrito = @distrito";
                }

                query += " ORDER BY id DESC";

                using SqlCommand cmd = new SqlCommand(query, conn);

                if (!string.IsNullOrWhiteSpace(rol) && rol.ToLower() == "departamental" && !string.IsNullOrWhiteSpace(departamento))
                    cmd.Parameters.AddWithValue("@departamento", departamento);
                else if (!string.IsNullOrWhiteSpace(rol) && rol.ToLower() == "distrital" && !string.IsNullOrWhiteSpace(distrito))
                    cmd.Parameters.AddWithValue("@distrito", distrito);

                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new
                    {
                        id = Convert.ToInt32(reader["id"]),
                        tipo_registro = reader["tipo_registro"]?.ToString() ?? "",
                        tipo_apoyo = reader["tipo_apoyo"]?.ToString() ?? "",
                        nombres = reader["nombres"]?.ToString() ?? "",
                        apellidos = reader["apellidos"]?.ToString() ?? "",
                        dui = reader["dui"]?.ToString() ?? "",
                        telefono = reader["telefono"]?.ToString() ?? "",
                        cargo = reader["cargo"]?.ToString() ?? "",
                        comunidad = reader["comunidad"]?.ToString() ?? "",
                        departamento = reader["departamento"]?.ToString() ?? "",
                        distrito = reader["distrito"]?.ToString() ?? "",
                        fecha = reader["fecha"] == DBNull.Value ? null : Convert.ToDateTime(reader["fecha"]).ToString("yyyy-MM-dd HH:mm:ss"),
                        creado_por = reader["creado_por"]?.ToString() ?? ""
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

                string query = @"INSERT INTO registros
                                (tipo_registro, tipo_apoyo, nombres, apellidos, dui, telefono, cargo, comunidad, departamento, distrito, fecha, creado_por)
                                VALUES
                                (@tipo_registro, @tipo_apoyo, @nombres, @apellidos, @dui, @telefono, @cargo, @comunidad, @departamento, @distrito, GETDATE(), @creado_por)";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@tipo_registro", nuevo.tipo_registro ?? "");
                cmd.Parameters.AddWithValue("@tipo_apoyo", nuevo.tipo_apoyo ?? "");
                cmd.Parameters.AddWithValue("@nombres", nuevo.nombres ?? "");
                cmd.Parameters.AddWithValue("@apellidos", nuevo.apellidos ?? "");
                cmd.Parameters.AddWithValue("@dui", nuevo.dui ?? "");
                cmd.Parameters.AddWithValue("@telefono", nuevo.telefono ?? "");
                cmd.Parameters.AddWithValue("@cargo", nuevo.cargo ?? "");
                cmd.Parameters.AddWithValue("@comunidad", nuevo.comunidad ?? "");
                cmd.Parameters.AddWithValue("@departamento", nuevo.departamento ?? "");
                cmd.Parameters.AddWithValue("@distrito", nuevo.distrito ?? "");
                cmd.Parameters.AddWithValue("@creado_por", nuevo.creado_por ?? "");

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

                string query = @"UPDATE registros
                                 SET tipo_registro = @tipo_registro,
                                     tipo_apoyo = @tipo_apoyo,
                                     nombres = @nombres,
                                     apellidos = @apellidos,
                                     dui = @dui,
                                     telefono = @telefono,
                                     cargo = @cargo,
                                     comunidad = @comunidad,
                                     departamento = @departamento,
                                     distrito = @distrito,
                                     creado_por = @creado_por
                                 WHERE id = @id";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@tipo_registro", datos.tipo_registro ?? "");
                cmd.Parameters.AddWithValue("@tipo_apoyo", datos.tipo_apoyo ?? "");
                cmd.Parameters.AddWithValue("@nombres", datos.nombres ?? "");
                cmd.Parameters.AddWithValue("@apellidos", datos.apellidos ?? "");
                cmd.Parameters.AddWithValue("@dui", datos.dui ?? "");
                cmd.Parameters.AddWithValue("@telefono", datos.telefono ?? "");
                cmd.Parameters.AddWithValue("@cargo", datos.cargo ?? "");
                cmd.Parameters.AddWithValue("@comunidad", datos.comunidad ?? "");
                cmd.Parameters.AddWithValue("@departamento", datos.departamento ?? "");
                cmd.Parameters.AddWithValue("@distrito", datos.distrito ?? "");
                cmd.Parameters.AddWithValue("@creado_por", datos.creado_por ?? "");

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

                string query = "DELETE FROM registros WHERE id = @id";

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

                int total = 0;
                int totalDepartamentos = 0;
                int totalDistritos = 0;

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM registros", conn))
                    total = Convert.ToInt32(cmd.ExecuteScalar());

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(DISTINCT departamento) FROM registros", conn))
                    totalDepartamentos = Convert.ToInt32(cmd.ExecuteScalar());

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(DISTINCT distrito) FROM registros", conn))
                    totalDistritos = Convert.ToInt32(cmd.ExecuteScalar());

                var porDepartamento = new List<object>();
                using (SqlCommand cmd = new SqlCommand(@"
            SELECT departamento, COUNT(*) total
            FROM registros
            GROUP BY departamento
            ORDER BY total DESC", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        porDepartamento.Add(new
                        {
                            departamento = reader["departamento"]?.ToString() ?? "",
                            total = Convert.ToInt32(reader["total"])
                        });
                    }
                }

                var topDistritos = new List<object>();
                using (SqlCommand cmd = new SqlCommand(@"
            SELECT TOP 10 distrito, COUNT(*) total
            FROM registros
            GROUP BY distrito
            ORDER BY total DESC, distrito ASC", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        topDistritos.Add(new
                        {
                            distrito = reader["distrito"]?.ToString() ?? "",
                            total = Convert.ToInt32(reader["total"])
                        });
                    }
                }

                var distritosBajos = new List<object>();
                using (SqlCommand cmd = new SqlCommand(@"
            SELECT TOP 3 distrito, COUNT(*) total
            FROM registros
            GROUP BY distrito
            ORDER BY total ASC, distrito ASC", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        distritosBajos.Add(new
                        {
                            distrito = reader["distrito"]?.ToString() ?? "",
                            total = Convert.ToInt32(reader["total"])
                        });
                    }
                }

                return Ok(new
                {
                    totalRegistros = total,
                    totalDepartamentos,
                    totalDistritos,
                    porDepartamento,
                    topDistritos,
                    distritosBajos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al cargar dashboard",
                    error = ex.Message
                });
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

                string query = @"SELECT id, tipo_registro, tipo_apoyo, nombres, apellidos, dui, telefono, cargo, comunidad, departamento, distrito, fecha, creado_por
                                 FROM registros
                                 ORDER BY id DESC";

                using SqlCommand cmd = new SqlCommand(query, conn);
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Registro
                    {
                        id = Convert.ToInt32(reader["id"]),
                        tipo_registro = reader["tipo_registro"]?.ToString() ?? "",
                        tipo_apoyo = reader["tipo_apoyo"]?.ToString() ?? "",
                        nombres = reader["nombres"]?.ToString() ?? "",
                        apellidos = reader["apellidos"]?.ToString() ?? "",
                        dui = reader["dui"]?.ToString() ?? "",
                        telefono = reader["telefono"]?.ToString() ?? "",
                        cargo = reader["cargo"]?.ToString() ?? "",
                        comunidad = reader["comunidad"]?.ToString() ?? "",
                        departamento = reader["departamento"]?.ToString() ?? "",
                        distrito = reader["distrito"]?.ToString() ?? "",
                        fecha = reader["fecha"] == DBNull.Value ? null : Convert.ToDateTime(reader["fecha"]),
                        creado_por = reader["creado_por"]?.ToString() ?? ""
                    });
                }

                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Registros");

                ws.Cell(1, 1).Value = "ID";
                ws.Cell(1, 2).Value = "Tipo Registro";
                ws.Cell(1, 3).Value = "Tipo Apoyo";
                ws.Cell(1, 4).Value = "Nombres";
                ws.Cell(1, 5).Value = "Apellidos";
                ws.Cell(1, 6).Value = "DUI";
                ws.Cell(1, 7).Value = "Teléfono";
                ws.Cell(1, 8).Value = "Cargo";
                ws.Cell(1, 9).Value = "Comunidad";
                ws.Cell(1, 10).Value = "Departamento";
                ws.Cell(1, 11).Value = "Distrito";
                ws.Cell(1, 12).Value = "Fecha";
                ws.Cell(1, 13).Value = "Creado Por";

                int fila = 2;
                foreach (var item in lista)
                {
                    ws.Cell(fila, 1).Value = item.id;
                    ws.Cell(fila, 2).Value = item.tipo_registro;
                    ws.Cell(fila, 3).Value = item.tipo_apoyo;
                    ws.Cell(fila, 4).Value = item.nombres;
                    ws.Cell(fila, 5).Value = item.apellidos;
                    ws.Cell(fila, 6).Value = item.dui;
                    ws.Cell(fila, 7).Value = item.telefono;
                    ws.Cell(fila, 8).Value = item.cargo;
                    ws.Cell(fila, 9).Value = item.comunidad;
                    ws.Cell(fila, 10).Value = item.departamento;
                    ws.Cell(fila, 11).Value = item.distrito;
                    ws.Cell(fila, 12).Value = item.fecha?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
                    ws.Cell(fila, 13).Value = item.creado_por;
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