import torch
import yaml

with open('../../../Properties/Config.yml') as f:
    config = yaml.safe_load(f)
    print(config)
    data_path: str = config['п»їdata_path']
    model_path: str = config['model_path']
    events_path: str = config['events_path']
    cameras_path: str = config['cameras_path']
    uploads_path: str = config['uploads_path']
    face_size: list[int] = config['face_size']
    supported_extensions: list[str] = config['supported_extensions']
    sources = config['sources']
    max_photo_count: int = 5
    min_detector_size: int = 128
    max_detector_size: int = 2048
    confidence_threshold: float = 0.99
    validation_threshold: float = 0.4
    device = torch.device('cuda:0' if torch.cuda.is_available() else 'cpu')
    print(torch.cuda.is_available())
    exit()
