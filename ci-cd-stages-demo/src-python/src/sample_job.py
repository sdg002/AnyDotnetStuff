from apscheduler.schedulers.blocking import BlockingScheduler
import datetime

sched = BlockingScheduler()

@sched.scheduled_job(trigger='cron', id='my_job_id_1', second=30)
def some_decorated_task_1():
    print(f"Inside first_job  current time is:{datetime.datetime.now()}")

@sched.scheduled_job(trigger='cron', id='my_job_id_2', second=45)
def some_decorated_task_2():
    print(f"Inside second_job  current time is:{datetime.datetime.now()}")

print("Hello world - this is an example of scheduled jobs via AP Scheduler")
all_jobs = sched.get_jobs()
print(f"Displaying all registered jobs:")
print(all_jobs)
sched.start()
    
