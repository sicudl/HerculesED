from flask import Flask, request, jsonify
from flask_restful import Api, Resource
from flask_apispec import doc, use_kwargs
from flask_apispec.views import MethodResource
from flask_apispec.extension import FlaskApiSpec
from apispec import APISpec
from apispec.ext.marshmallow import MarshmallowPlugin
from marshmallow import Schema, fields, ValidationError

from similarity import SimilarityService, RO, Ranking, ROIdError, ROTypeError
import ro_storage_memory
import ro_cache_memory

import logging
import json
import pdb

logging.basicConfig()
logger = logging.getLogger('SIMILARITY_API')
logger.setLevel(logging.DEBUG)

app = Flask(__name__)
app.logger.setLevel(logging.DEBUG)
api = Api(app)

app.config.update({
    'APISPEC_SPEC': APISpec(
        title='Hercules REST API to retrieve similar ROs',
        version='0.1',
        openapi_version='3.0.0',
        info=dict(
            description='This module receives a collection of ROs. It then returns the most similar ROs of a given RO.'
        ),
        plugins=[MarshmallowPlugin()]
    ),
    'APISPEC_SWAGGER_URL': '/doc-json/',  # URI to access API Doc JSON 
    'APISPEC_SWAGGER_UI_URL': '/doc/'  # URI to access UI of API Doc
})
docs = FlaskApiSpec(app)


# Type definitions

class SimilarityAddSchema(Schema):
    ro_id = fields.String(required=True, description="ID of the research object")
    ro_type = fields.String(required=True, description="Type of the research object: 'research_paper', 'code_project', 'protocol'")
    text = fields.String(required=True, description="Concatenation of title and abstract of the RO")
    authors = fields.String(required=True, description="Authors of the paper in a single string")
    thematic_descriptors = fields.List(fields.List(fields.Raw), required=True, description="List of pairs composed by thematic descriptors returned by the enrichment API and their probabilities.")
    specific_descriptors = fields.List(fields.List(fields.Raw), required=True, description="List of pairs composed by specific descriptors returned by the enrichment API and their probabilities.")

class SimilarityQuerySchema(Schema):
    ro_id = fields.String(required=True, description="ID of the research object")
    ro_type_target = fields.String(required=True, description="Type of the similar research objects to return: 'research_paper', 'code_project', 'protocol'")


# Service-level object instances

db = ro_storage_memory.MemoryROStorage()
cache = ro_cache_memory.MemoryROCache()
similarity = SimilarityService(db, cache)
    
# Endpoint classes
    
class SimilarityAddAPI(MethodResource, Resource):
    
    #decorators = [auth.login_required]
    @doc(description='Hercules similarity API: Add new research objects.',
         tags=['Hercules', 'similarity'])
    @use_kwargs(SimilarityAddSchema, location='json')
    #@marshal_with(SimilarityAddResponseSchema, description="")  # marshalling with marshmallow
    def post(self, **kwargs):
        logger.debug(kwargs)
        ro = RO(kwargs['ro_id'], kwargs['ro_type'])
        ro.text = kwargs['text']
        similarity.generate_embedding(ro)
        similarity.add_ro(ro)
        logger.debug("RO added")
        

class SimilarityQueryAPI(MethodResource, Resource):
    
    #decorators = [auth.login_required]
    @doc(description='Hercules similarity API: Query similar ROs.',
         tags=['Hercules', 'similarity'])
    @use_kwargs(SimilarityQuerySchema, location='json')
    #@marshal_with(SimilarityQueryResponseSchema, description="")  # marshalling with marshmallow
    def get(self, **kwargs):
        logger.debug(kwargs)
        similar_ro_ids = similarity.get_ro_ranking(kwargs['ro_id'], kwargs['ro_type_target'])            
        logger.debug(f"Similar ROs: {similar_ro_ids}")
        return jsonify(similar_ro_ids)


# Declare endpoints and documentation for the API

api.add_resource(SimilarityAddAPI, '/add_ro')
api.add_resource(SimilarityQueryAPI, '/query_similar')

docs.register(SimilarityAddAPI)
docs.register(SimilarityQueryAPI)


# Error handlers

@app.errorhandler(ROIdError)
def handle_bad_request(e):
    return f"RO not found", 404


if __name__ == "__main__":
    app.run()
