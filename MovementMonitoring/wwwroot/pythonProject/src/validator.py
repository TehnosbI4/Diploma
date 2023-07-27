import json
import uuid
from enum import Enum

from pika.adapters.blocking_connection import BlockingChannel

from faceDetector import FaceDetector
from person import Person, PersonData

import dataManager as dataMgr
import embedder as emb
import embeddingsTable as embTable


class InputType(Enum):
    CAM = 0
    VIDEO = 1
    IMAGE = 2


class Validator:
    def __init__(self, source_cap, source_id: str, source_type: InputType, face_detector: FaceDetector,
                 channel: BlockingChannel, validation_threshold=0.5):
        self.__source_id = source_id
        self.__source_cap = source_cap
        self.__source_type = source_type
        self.__validation_threshold = validation_threshold
        self.__face_detector = face_detector
        self.__channel = channel

    def validate(self):
        print(f'Process> validate for source {self.__source_id} started...')
        time = dataMgr.get_formatted_datetime()
        frame = self.__get_frame()
        faces = self.__face_detector.detect_all_faces(frame)
        if faces is not None:
            dataMgr.write_camera_capture(self.__source_id, time, frame)
            response = {
                'SourceId': self.__source_id,
                'Time': time,
                'ValidationThreshold': self.__validation_threshold
            }
            persons = []
            for face in faces:
                embedding = emb.get_embedding(face)
                if embedding is not None:
                    current_person = Person('tmp_person')
                    current_person_pd = PersonData(embedding, '', face)
                    current_person.add_data(current_person_pd)
                    most_similar_person, most_similar_data, similarity = embTable.most_similar_person(
                        current_person)
                    if most_similar_person is not None:
                        if similarity >= self.__validation_threshold:
                            if most_similar_person.guid is not None:
                                last_photo_path = dataMgr.write_event(most_similar_person.guid,
                                                                      self.__source_id,
                                                                      time, face)
                                person_data = {'Guid': most_similar_person.guid,
                                               'LastPhotoPath': last_photo_path,
                                               'Validated': True,
                                               'MostSimilarGuid': most_similar_person.guid,
                                               'MostSimilarPhotoPath': most_similar_data.path,
                                               'Similarity': similarity}
                                if not embTable.check_max_count(most_similar_person):
                                    path = dataMgr.write_image_by_guid(most_similar_person.guid, face)
                                    most_similar_person.add_image(embedding, path, face)
                                if most_similar_person.guid != 'tmp_person':
                                    persons.append(person_data)
                        else:
                            guid = str(uuid.uuid4())
                            last_photo_path = dataMgr.write_event(guid, self.__source_id, time, face)
                            person_data = {'Guid': guid,
                                           'LastPhotoPath': last_photo_path,
                                           'Validated': False,
                                           'MostSimilarGuid': most_similar_person.guid,
                                           'MostSimilarPhotoPath': most_similar_data.path,
                                           'Similarity': similarity}
                            current_path = dataMgr.write_image_by_guid(guid, face)
                            current_person.guid = guid
                            current_person_pd.path = current_path
                            if current_person.guid != 'tmp_person':
                                persons.append(person_data)
                                embTable.add_person(current_person)
            response['DetectedPersons'] = persons
            json_data = json.dumps(response, indent=4, default=str)
            # with open('data.json', 'w') as f:
            #     json.dump(response, f, ensure_ascii=False, indent=4)
            self.__channel.basic_publish(exchange='', routing_key='validator', body=json_data)
            return json_data

    def __get_frame(self):
        if self.__source_type == InputType.CAM or self.__source_type == InputType.VIDEO:
            if not self.__source_cap.isOpened():
                print(f'Error> Error while trying to read {self.__source_type}!')
                # raise Exception(f'Cannot read input {self.__source_type} data!')
            else:
                ret, frame = self.__source_cap.read()
                if ret:
                    # cv2.imwrite('frame.png', frame)
                    return frame
                return None
        elif self.__source_type == InputType.IMAGE:
            return self.__source_cap
        return None
