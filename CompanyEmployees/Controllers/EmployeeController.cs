﻿using AutoMapper;
using CompanyEmployees.ActionFilters;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies/{CompanyId}/employees")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IDataShaper<EmployeeDTO> _dataShaper;

        public EmployeeController(IRepositoryManager repository,
                                  ILoggerManager logger,
                                  IMapper mapper,
                                  IDataShaper<EmployeeDTO> dataShaper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _dataShaper = dataShaper;
        }

        [HttpGet]
        [HttpHead]
        public async Task<IActionResult> GetEmployeesForCompany(Guid companyId,
            [FromQuery] EmployeeParameters employeeParameters)
        {
            if (!employeeParameters.ValidAgeRange)
                return BadRequest("Max age can't be less than min age");

            var company = await _repository.Company.GetCompanyAsync(companyId,
                          trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId}" +
                    $" doesn't exist in the database");
                return NotFound();
            }

            var employeesFromDb = await _repository.Employee.GetEmployeesAsync(companyId,
                                    employeeParameters, trackChanges: false);

            Response.Headers.Add("X-Pagination",
                JsonConvert.SerializeObject(employeesFromDb.MetaData));

            var employeesDTO = _mapper.Map<IEnumerable<EmployeeDTO>>(employeesFromDb);

            return Ok(_dataShaper.ShapeData(employeesDTO, employeeParameters.Fields));
        }

        [HttpGet("{id}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = await _repository.Company.GetCompanyAsync(
                companyId, trackChanges: false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id : {companyId} doesn't exist" +
                    $"in the database.");
                return NotFound();
            }

            var employeeDb = await _repository.Employee.GetEmployeeAsync
                            (companyId, id,trackChanges: false);

            if (employeeDb == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the" +
                    $" database");
                return NotFound();
            }

            var employee = _mapper.Map<EmployeeDTO>(employeeDb);

            return Ok(employee);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId,
                                        [FromBody] EmployeeForCreationDTO employee)
        {
            if (employee == null)
            {
                _logger.LogError("EmployeeForCreationDTO object sent" +
                                 " from client is null");
                return BadRequest("EmployeeForCreationDto object is null");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the EmployeeCreationDTO" +
                    "object");
                return UnprocessableEntity(ModelState);
            }

            var company = await _repository.Company.GetCompanyAsync(companyId,
                                                         trackChanges: false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist" +
                                $"in the databse.");
                return NotFound();
            }

            var employeeEntity = _mapper.Map<Employee>(employee);

            _repository.Employee.CreateEmployeeForCompany(companyId,
                                                          employeeEntity);

            await _repository.SaveAsync();

            var employeeToReturn = _mapper.Map<EmployeeDTO>(employeeEntity);

            return CreatedAtRoute("GetEmployeeForCompany",
                                   new { companyId, id = employeeToReturn.Id },
                                   employeeToReturn);
        }

        [HttpDelete("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteEmployeeForCompany
        (Guid companyId, Guid id)
        {
            var company = await _repository.Company.GetCompanyAsync(companyId,
                                                        trackChanges: false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId}" +
                    $"doesn't exist in the database.");
                return NotFound();
            }

            //[ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
            //handles the commented out code below

            /*var employeeForCompany = await _repository.Employee.GetEmployeeAsync(
                                    companyId, id, trackChanges: false);

            if (employeeForCompany == null)
            {
                _logger.LogInfo($"Employee with id: {id}" +
                    $"doesn't exist in the database.");
                return NotFound();
            }*/

            var employeeForCompany = HttpContext.Items["employee"] as Employee;

            _repository.Employee.DeleteEmployee(employeeForCompany);
            await _repository.SaveAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany
        (Guid companyId, Guid id,[FromBody] EmployeeForUpdateDTO employee)
        {
            if(employee == null)
            {
                _logger.LogError("EmployeeForUpdateDto object " +
                    "sent from client is null.");
                return BadRequest("EmployeeForUpdateDto object is null");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the " +
                    "EmployeeForUpdateDTO object");
                return UnprocessableEntity(ModelState);
            }

            var company = await _repository.Company.GetCompanyAsync(companyId,
                                                    trackChanges: false);

            if(company == null)
            {
                _logger.LogInfo($"Company with id: {companyId}" +
                                "doesn't exist in the database.");
                return NotFound();
            }

            //trackChanges: true
                //as soon as any change is made on any property of the entity,
                //EF Core will set the state of that entity to Modified

            //[ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
                //handles the commented code below
            /*var employeeEntity = await _repository.Employee.GetEmployeeAsync
                                (companyId, id,trackChanges: true);

            if(employeeEntity == null)
            {
                _logger.LogInfo($"Employee with id: {id}" +
                                " doesn't exist in the database.");
                return NotFound();
            }*/

            var employeeEntity = HttpContext.Items["employee"] as Employee;

            //state of employeeEntity changes to Modified after the following mapping
            _mapper.Map(employee, employeeEntity);
            await _repository.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany
        (Guid companyId,Guid id, 
        [FromBody] JsonPatchDocument<EmployeeForUpdateDTO> patchDoc)
        {
            if(patchDoc == null)
            {
                _logger.LogError("patchDoc object sent from client is null.");
                return BadRequest("patchDoc object is null");
            }

            //[ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
                //handles the code commented out below
            /*var company = await _repository.Company.GetCompanyAsync(companyId,
                                                        trackChanges: false);

            if(company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't" +
                    "exist in the database.");
                return NotFound();
            }

            var employeeEntity = await _repository.Employee.GetEmployeeAsync
                                    (companyId, id, trackChanges: true);

            if (employeeEntity == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't " +
                                "exist in the database.");
                return NotFound();
            }*/

            var employeeEntity = HttpContext.Items["employee"] as Employee;

            var employeeToPatch = _mapper.Map<EmployeeForUpdateDTO>(employeeEntity);

            patchDoc.ApplyTo(employeeToPatch, ModelState);

            //validate using DTO validation attributes
            //without this remove operation can be applied to fields like age
            //setting it to 0 which does not adhere to the range attribute
            TryValidateModel(employeeToPatch);

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the patch document");
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(employeeToPatch, employeeEntity);

            await _repository.SaveAsync();

            return NoContent();

        }

    }
}
