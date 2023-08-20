FROM python:3.9.17
WORKDIR /python-docker
COPY requirements.txt requirements.txt
RUN pip3 install -r requirements.txt
COPY . .
CMD [ "python" ,  "/python-docker/src/sample_job.py"] #does not produce any output
