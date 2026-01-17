from django.urls import path
from . import views

urlpatterns = [
    path("", views.index, name="index"),
    path("api/facerec/", views.facerec_api, name="facerec_api"),
]
