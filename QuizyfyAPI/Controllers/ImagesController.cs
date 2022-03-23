using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using QuizyfyAPI.Contracts.Responses;
using QuizyfyAPI.Services;

namespace QuizyfyAPI.Controllers;
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("multipart/form-data")]
[ApiController]
public class ImagesController : ControllerBase
{
    private readonly IImageService _imageService;

    public ImagesController(IImageService imageService)
    {
        _imageService = imageService;
    }

    /// <summary>
    /// Get list of all images.
    /// </summary>
    /// <returns>An ActionResult of ImageModel array type</returns>
    /// <remarks>
    /// Sample request (this request returns **array of images**)  
    ///      
    ///     GET /images
    ///     
    /// </remarks>
    /// <response code="200">Returns array of all images</response>
    /// <response code="204">No images exists so return nothing.</response>
    /// <response code="404">Images don't exist.</response>
    /// <response code="406">Request data type is not in acceptable format.</response>
    /// <response code="422">Request data couldn't be processed.</response>
    /// <response code="500">Something threw exception on server.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<ActionResult<ImageResponse[]>> Get()
    {
        var getAllResponse = await _imageService.GetAll();

        if (!getAllResponse.Success)
        {
            if (!getAllResponse.Found)
            {
                return NotFound(getAllResponse.Errors);
            }
            return NoContent();
        }
        return getAllResponse.Object;
    }

    /// <summary>
    /// Get one image with given id.
    /// </summary>
    /// <param name="id">Parameter which tells us image to return.</param>
    /// <returns>An ActionResult of ImageModel type</returns>
    /// <remarks>
    /// Sample request (this request returns **one image**)  
    ///      
    ///     GET /images/1
    ///     
    /// </remarks>
    /// <response code="200">Returns one image</response>
    /// <response code="204">No images exists so return nothing.</response>
    /// <response code="404">Image doesn't exist.</response>
    /// <response code="406">Request data type is not in acceptable format.</response>
    /// <response code="422">Request data couldn't be processed.</response>
    /// <response code="500">Something threw exception on server.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}")]
    public async Task<ActionResult<ImageResponse>> Get(int id)
    {
        var getResponse = await _imageService.Get(id);

        if (!getResponse.Found)
        {
            return NotFound(getResponse.Errors);
        }
        return getResponse.Object;
    }

    /// <summary>
    /// Create image with provided info.
    /// </summary>
    /// <param name="file">This is representation of file you want to upload</param>
    /// <returns>>An ActionResult of ImageModel</returns>
    /// <remarks>
    /// Sample request (this request returns **created image**)  
    ///      
    ///     POST /images
    ///     
    /// </remarks>  
    /// <response code="201">Image was created and you can access it.</response>
    /// <response code="400">Data provided was not complete or corrupted.</response>
    /// <response code="406">Request data type is not in acceptable format.</response>
    /// <response code="422">Request data couldn't be processed.</response>
    /// <response code="500">Something threw exception on server.</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<ActionResult<ImageResponse>> Post(IFormFile image)
    {
        var createResponse = await _imageService.Create(image);

        if (!createResponse.Success)
        {
            return BadRequest(createResponse.Errors);
        }
        return CreatedAtAction(nameof(Get), new { id = createResponse.Object.Id }, createResponse.Object);
    }

    /// <summary>
    /// Updates image with given id with specified  data.
    /// </summary>
    /// <param name="id">Id of image you want to update.</param>
    /// <param name="file">New image.</param>
    /// <returns>>An ActionResult of ImageModel</returns>
    /// <remarks>
    /// Sample request (this request returns **updated image**)  
    ///      
    ///     PUT /images/1
    ///     
    /// </remarks>  
    /// <response code="200">Returns image with provided id and updated info.</response>
    /// <response code="204">Probably should never return that but there is possibility that images isn't null but mapping result in null.</response> 
    /// <response code="400">Bad request not complete or corrupted data.</response>
    /// <response code="404">Image with provided id wasn't found.</response> 
    /// <response code="406">Request data type is not in acceptable format.</response>
    /// <response code="422">Request data couldn't be processed.</response>
    /// <response code="500">Something threw exception on server.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesDefaultResponseType]
    [HttpPut("{id}")]
    public async Task<ActionResult<ImageResponse>> Put(int id, IFormFile file)
    {
        var updateResponse = await _imageService.Update(id, file);

        if (!updateResponse.Success)
        {
            if (!updateResponse.Found)
            {
                return NotFound(updateResponse.Errors);
            }
            return BadRequest(updateResponse.Errors);
        }
        return updateResponse.Object;
    }

    /// <summary>
    /// Deletes image with specified id.
    /// </summary>
    /// <param name="id">Specifies which image to delete</param>
    /// <returns>Response Code</returns>
    /// <remarks>
    /// Sample request (this request returns **response code only**)  
    ///      
    ///     DELETE /images/1
    ///     
    /// </remarks> 
    /// <response code="200">Image was sucessfully deleted.</response> 
    /// <response code="400">Request data was not complete or corrupted.</response> 
    /// <response code="404">Image with provided id wasn't found.</response> 
    /// <response code="406">Request data type is not in acceptable format.</response>
    /// <response code="422">Request data couldn't be processed.</response>
    /// <response code="500">Something threw exception on server.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleteResponse = await _imageService.Delete(id);

        if (!deleteResponse.Success)
        {
            if (!deleteResponse.Found)
            {
                return NotFound(deleteResponse.Errors);
            }
            return BadRequest(deleteResponse.Errors);
        }
        return Ok();
    }
}
