from json import JSONEncoder

class VPPage:
    def __init__(self, path: str, content: str, title: str, file_path: str, toc_index: str | None) -> None:
        self.path = path
        self.content = content
        self.title = title
        self.file_path = file_path
        self.toc_index = toc_index

    path: str
    content: str
    title: str
    file_path: str
    toc_index: str | None

    class VPPageJsonEncoder(JSONEncoder):
        """
        JsonEncoder for VPPage
        """
        def default(self, o):
            return o.__dict__
