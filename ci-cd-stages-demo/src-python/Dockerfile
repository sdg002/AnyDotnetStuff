FROM python:3.9.17
WORKDIR /python-docker
COPY requirements.txt requirements.txt
RUN pip3 install -r requirements.txt
COPY . .
#CMD [ "python" ,  "--version"] #this works
CMD [ "python" ,  "/python-docker/src/sample_job.py"] #does not produce any output
#CMD [ "python" ,  "/python-docker/src/main.py"] #this works
# ENV STATIC_URL /static
# ENV STATIC_PATH /var/www/app/static
# COPY ./requirements.txt /var/www/requirements.txt
# RUN pip install -r /var/www/requirements.txt