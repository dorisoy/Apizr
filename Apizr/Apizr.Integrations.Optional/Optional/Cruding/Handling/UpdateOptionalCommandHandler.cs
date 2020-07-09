﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Apizr.Mapping;
using Apizr.Mediation.Cruding.Handling.Base;
using Apizr.Requesting;
using MediatR;
using Optional;
using Optional.Async.Extensions;

namespace Apizr.Optional.Cruding.Handling
{
    public class UpdateOptionalCommandHandler<TModelEntity, TApiEntity, TApiEntityKey, TReadAllResult, TReadAllParams> : 
        UpdateCommandHandlerBase<TModelEntity, TApiEntity, TApiEntityKey, TReadAllResult, TReadAllParams, UpdateOptionalCommand<TApiEntityKey, TModelEntity>, Option<Unit, ApizrException>>
        where TModelEntity : class
        where TApiEntity : class
    {
        public UpdateOptionalCommandHandler(IApizrManager<ICrudApi<TApiEntity, TApiEntityKey, TReadAllResult, TReadAllParams>> crudApiManager, IMappingHandler mappingHandler) : base(crudApiManager, mappingHandler)
        {
        }

        public override Task<Option<Unit, ApizrException>> Handle(UpdateOptionalCommand<TApiEntityKey, TModelEntity> request, CancellationToken cancellationToken)
        {
            try
            {
                return request
                        .SomeNotNull(new ApizrException(new NullReferenceException($"Request {request.GetType().GetFriendlyName()} can not be null")))
                        .MapAsync(_ =>
                            CrudApiManager
                                .ExecuteAsync((ct, api) => api.Update(request.Key, Map<TModelEntity, TApiEntity>(request.Payload), ct), cancellationToken)
                                .ContinueWith(task => Unit.Value, cancellationToken));
            }
            catch (ApizrException e)
            {
                return Task.FromResult(Option.None<Unit, ApizrException>(e));
            }
        }
    }

    public class UpdateOptionalCommandHandler<TModelEntity, TApiEntity, TReadAllResult, TReadAllParams> : 
        UpdateCommandHandlerBase<TModelEntity, TApiEntity, TReadAllResult, TReadAllParams, UpdateOptionalCommand<TModelEntity>, Option<Unit, ApizrException>>
        where TModelEntity : class
        where TApiEntity : class
    {
        public UpdateOptionalCommandHandler(IApizrManager<ICrudApi<TApiEntity, int, TReadAllResult, TReadAllParams>> crudApiManager, IMappingHandler mappingHandler) : base(crudApiManager, mappingHandler)
        {
        }

        public override Task<Option<Unit, ApizrException>> Handle(UpdateOptionalCommand<TModelEntity> request, CancellationToken cancellationToken)
        {
            try
            {
                return request
                        .SomeNotNull(new ApizrException(new NullReferenceException($"Request {request.GetType().GetFriendlyName()} can not be null")))
                        .MapAsync(_ =>
                            CrudApiManager
                                .ExecuteAsync((ct, api) => api.Update(request.Key, Map<TModelEntity, TApiEntity>(request.Payload), ct), cancellationToken)
                                .ContinueWith(task => Unit.Value, cancellationToken));
            }
            catch (ApizrException e)
            {
                return Task.FromResult(Option.None<Unit, ApizrException>(e));
            }
        }
    }
}