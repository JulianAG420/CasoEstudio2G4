using CasoEstudio2.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;


namespace CasoEstudio2.Controllers
{
    public class CasasController : Controller
    {
        private readonly string _connectionString;

        public CasasController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Consulta()
        {
            IEnumerable<CasasModel> casas;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                casas = connection.Query<CasasModel>("ConsultarCasas").ToList();
            }
            return View(casas);
        }

        public IActionResult Alquilar()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
               
                var casasDisponibles = connection.Query<CasasModel>(
                    "SELECT IdCasa, DescripcionCasa, PrecioCasa FROM CasasSistema WHERE UsuarioAlquiler IS NULL").ToList();

                ViewBag.CasasDisponibles = casasDisponibles; 
            }
            return View();
        }

        [HttpPost]
        public IActionResult Alquilar(long idCasa, string usuarioAlquiler)
        {
            if (string.IsNullOrEmpty(usuarioAlquiler))
            {
                TempData["Error"] = "Debe ingresar un nombre de usuario.";
                return RedirectToAction("Alquilar");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                try
                {
                    
                    connection.Execute("AlquilarCasa", new
                    {
                        IdCasa = idCasa,
                        UsuarioAlquiler = usuarioAlquiler,
                        FechaAlquiler = DateTime.Now 
                    }, commandType: System.Data.CommandType.StoredProcedure);

                    TempData["Success"] = "La casa se alquiló exitosamente.";
                }
                catch (SqlException ex)
                {
                    
                    TempData["Error"] = ex.Message;
                }
            }
            return RedirectToAction("Consulta");
        }

    }
}