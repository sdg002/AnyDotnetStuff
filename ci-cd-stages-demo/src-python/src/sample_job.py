from apscheduler.schedulers.blocking import BlockingScheduler
import datetime
import logging

logging.basicConfig(level=logging.INFO)
print("Going to create an instance of BlockingScheduler")
sched = BlockingScheduler()
print("After creating an instance of BlockingScheduler")

@sched.scheduled_job(trigger='cron', id='my_job_id_1', second=30)
def some_decorated_task_1():
    logging.info(f"Inside first_job  current time is:{datetime.datetime.now()}")

@sched.scheduled_job(trigger='cron', id='my_job_id_2', second=45)
def some_decorated_task_2():
    logging.info(f"Inside second_job  current time is:{datetime.datetime.now()}")

print("Before calling get_jobs")
all_jobs = sched.get_jobs()
print(f"Displaying all registered jobs:")
print(all_jobs)
sched.start()
    
