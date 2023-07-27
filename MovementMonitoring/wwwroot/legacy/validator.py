import json
import uuid
from enum import Enum

from pika.adapters.blocking_connection import BlockingChannel

from dataManager import DataManager
from embedder import Embedder
from embeddingsTable import EmbeddingsTable
from faceDetector import FaceDetector
from person import Person, PersonData


class InputType(Enum):
    CAM = 0
    VIDEO = 1
    IMAGE = 2


class Validator:
    def __init__(self, source_id, source_cap, source_type: InputType, embedder: Embedder,
                 embeddings_table: EmbeddingsTable,
                 face_detector: FaceDetector, data_manager: DataManager, channel: BlockingChannel,
                 validation_threshold=0.5):
        self.__source_id = source_id
        self.__source_cap = source_cap
        self.__source_type = source_type
        self.__validation_threshold = validation_threshold
        self.__embedder = embedder
        self.__embeddings_table = embeddings_table
        self.__face_detector = face_detector
        self.__data_manager = data_manager
        self.__channel = channel

    def validate(self):
        print(f'Process> validate for source {self.__source_id} started...')
        time = self.__data_manager.get_formatted_datetime()
        frame = self.__get_frame()
        faces = self.__face_detector.detect_all_faces(frame)
        if faces is not None:
            self.__data_manager.write_camera_capture(self.__source_id, time, frame)
            response = {
                'SourceId': self.__source_id,
                'Time': time,
                'ValidationThreshold': self.__validation_threshold
            }
            persons = []
            for face in faces:
                embedding = self.__embedder.get_embedding(face)
                if embedding is not None:
                    current_person = Person('tmp_person')
                    current_person_pd = PersonData(embedding, '', face)
                    current_person.add_data(current_person_pd)
                    most_similar_person, most_similar_data, similarity = self.__embeddings_table.most_similar_person(
                        current_person)
                    if most_similar_person is not None:
                        if similarity >= self.__validation_threshold:
                            last_photo_path = self.__data_manager.write_event(most_similar_person.guid,
                                                                              self.__source_id,
                                                                              time, face)
                            person_data = {'Guid': most_similar_person.guid,
                                           'LastPhotoPath': last_photo_path,
                                           'Validated': True,
                                           'MostSimilarGuid': most_similar_person.guid,
                                           'MostSimilarPhotoPath': most_similar_data.path,
                                           'Similarity': similarity}
                            if not self.__embeddings_table.check_max_count(most_similar_person):
                                path = self.__data_manager.write_image_by_guid(most_similar_person.guid, face)
                                most_similar_person.add_image(embedding, path, face)
                            persons.append(person_data)
                        else:
                            guid = str(uuid.uuid4())
                            last_photo_path = self.__data_manager.write_event(guid, self.__source_id, time, face)
                            person_data = {'Guid': guid,
                                           'LastPhotoPath': last_photo_path,
                                           'Validated': False,
                                           'MostSimilarGuid': most_similar_person.guid,
                                           'MostSimilarPhotoPath': most_similar_data.path,
                                           'Similarity': similarity}
                            current_path = self.__data_manager.write_image_by_guid(guid, face)
                            current_person.guid = guid
                            current_person_pd.path = current_path
                            persons.append(person_data)
                            self.__embeddings_table.add_person(current_person)
            response['DetectedPersons'] = persons
            json_data = json.dumps(response, indent=4, default=str)
            # with open('data.json', 'w') as f:
            #     json.dump(response, f, ensure_ascii=False, indent=4)
            self.__channel.basic_publish(exchange='', routing_key='validator', body=json_data)
            return json_data

    def __get_frame(self):
        if self.__source_type == InputType.CAM or InputType.VIDEO:
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
