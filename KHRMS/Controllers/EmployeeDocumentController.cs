using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Reflection.Metadata;
using System.Xml.Linq;


namespace KHRMS
{

    /// <summary>
    /// API Controller for managing Employee Documents Information.
    /// Provides endpoints to Create, Read, Update, and Delete employee documents records.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeDocumentController(IEmployeeDocumentService employeeDocumentService, ILogger<EmployeeDocumentController> logger) : ControllerBase

    {
        private readonly IEmployeeDocumentService _employeeDocumentService = employeeDocumentService;
        private readonly List<string> _allowedExtensions = new List<string> { ".doc", ".docx", ".xaml" };
        // Define document as a global variable
        private EmployeeDocumentInfo _document;


        [HttpGet("GetAllDocumentsInfo")]
        public async Task<ActionResult<IEnumerable<EmployeeDocumentInfo>>> GetAll()
        {
            var documents = await _employeeDocumentService.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<EmployeeDocumentInfo>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Employee Documents information retrieved successfully.",
                Data = documents
            });
        }

        /// <summary>
        /// Retrieves employee document information by ID.
        /// </summary>
        /// <param name="id">Employee Document Info ID</param>

        [HttpGet("GetDocument/{id}")]
        public async Task<ActionResult<EmployeeDocumentInfo>> GetDocument(long id)
        {
            var document = await _employeeDocumentService.GetByIdAsync(id);
            if (document == null)
            {
                return NotFound(new ApiResponse<EmployeeDocumentInfo>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.EmployeeDocumentNotFound,
                    Data = null
                }); ;
            }
            return Ok(new ApiResponse<EmployeeDocumentInfo>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmployeeDocumentFound,
                Data = document
            });
        }


        /// <summary>
        /// Creates a new employee Document information record.
        /// </summary>
        /// <param Id="employeeId" file="IFormFile" )>Employee Document Info object</param>


        [HttpPost("Upload Document")]
        public async Task<IActionResult> Create([FromForm] long employeeId, string documentName, IFormFile file)
        {
            if (file == null)
            {
                return BadRequest("File is not provided.");
            }

            if (file.Length == 0)
            {
                return BadRequest("File is empty.");
            }
            if (string.IsNullOrWhiteSpace(documentName))
            {
                return BadRequest("Document name is required.");
            }
            var extension = Path.GetExtension(file.FileName)?.ToLower();
            if (extension != ".pdf" && extension != ".docx")
            {
                return BadRequest("Only .pdf and .docx files are allowed.");
            }

            try
            {
                var filePath = Path.Combine("uploads", Path.GetFileName(file.FileName));

                // Ensure the uploads directory exists
                if (!Directory.Exists("uploads"))
                {
                    Directory.CreateDirectory("uploads");
                }
                // Save the file to the server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var document = new EmployeeDocumentInfo
                {
                    EmployeeId = employeeId,
                    FilePath = filePath,
                    DocumentName = file.FileName,
                };

                await _employeeDocumentService.AddAsync(document);
                //  return CreatedAtAction(nameof(GetDocument), new { id = _document.Id }, _document);
                return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        /// <summary>
        /// View an employee payment information record by ID.
        /// </summary>
        /// <param name="id">Employee Document Info ID</param>     
        /// 
        [HttpGet("view/{id}")]
        public async Task<IActionResult> ViewFile(long id)
        {

            var document = await _employeeDocumentService.GetByIdAsync(id);
            if (document == null)
            {
                //  _logger.LogWarning("Document with ID: {DocumentId} not found.", id);
                return NotFound();
            }

            var filePath = document.FilePath;
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var extension = Path.GetExtension(filePath)?.ToLower();
            var contentType = extension == ".pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            return File(fileBytes, contentType, Path.GetFileName(filePath));


        }
        /// <summary>
        /// Deletes an employee payment information record by ID.
        /// </summary>
        /// <param name="id">Employee Document Info ID</param>     
        /// 
        [HttpDelete("DeleteDocument/{id}")]
        public async Task<ActionResult> DeleteDocument(long id)
        {
            var document = await _employeeDocumentService.GetByIdAsync(id);
            if (document == null)
                return NotFound();

            await _employeeDocumentService.DeleteAsync(id);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.DocumentRequestDeleted,
                Data = true
            });
        }
    }
}



