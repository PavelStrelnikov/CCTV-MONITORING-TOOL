import logging

from fastapi import APIRouter, Depends, HTTPException
from pydantic import BaseModel
from sqlalchemy.exc import IntegrityError
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.api.deps import get_session
from cctv_monitor.api.schemas import FolderCreate, FolderUpdate, FolderTreeOut, FolderOut
from cctv_monitor.storage.repositories import FolderRepository

logger = logging.getLogger(__name__)

router = APIRouter(tags=["folders"])


@router.get("/folders", response_model=list[FolderTreeOut])
async def list_folders(session: AsyncSession = Depends(get_session)):
    repo = FolderRepository(session)
    return await repo.get_tree()


@router.post("/folders", response_model=FolderOut, status_code=201)
async def create_folder(
    body: FolderCreate,
    session: AsyncSession = Depends(get_session),
):
    repo = FolderRepository(session)
    try:
        folder = await repo.create(name=body.name, parent_id=body.parent_id, color=body.color, icon=body.icon)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    await session.commit()
    return FolderOut(
        id=folder.id, name=folder.name,
        parent_id=folder.parent_id, sort_order=folder.sort_order,
        color=folder.color, icon=folder.icon,
    )


class _FolderReorderItem(BaseModel):
    id: int
    sort_order: int


class _FolderReorderRequest(BaseModel):
    items: list[_FolderReorderItem]


@router.put("/folders/reorder")
async def reorder_folders(
    body: _FolderReorderRequest,
    session: AsyncSession = Depends(get_session),
):
    repo = FolderRepository(session)
    for item in body.items:
        await repo.update(item.id, sort_order=item.sort_order)
    await session.commit()
    return {"ok": True}


@router.patch("/folders/{folder_id}", response_model=FolderOut)
async def update_folder(
    folder_id: int,
    body: FolderUpdate,
    session: AsyncSession = Depends(get_session),
):
    repo = FolderRepository(session)
    fields = body.model_dump(exclude_unset=True)
    logger.info("folder.update folder_id=%s fields=%s", folder_id, fields)
    if not fields:
        raise HTTPException(status_code=400, detail="No fields to update")

    # Validate parent_id change
    if "parent_id" in fields:
        new_parent_id = fields["parent_id"]
        if new_parent_id is not None:
            if new_parent_id == folder_id:
                raise HTTPException(status_code=400, detail="Folder cannot be its own parent")
            parent = await repo.get_by_id(new_parent_id)
            if parent is None:
                raise HTTPException(status_code=400, detail="Parent folder not found")
            if parent.parent_id is not None:
                raise HTTPException(status_code=400, detail="Maximum folder depth is 2 levels")
            # Check that the folder being moved has no children (would exceed 2-level depth)
            children = await repo.get_children(folder_id)
            if children:
                raise HTTPException(
                    status_code=400,
                    detail="Cannot move folder with sub-folders under another folder",
                )

    folder = await repo.update(folder_id, **fields)
    if folder is None:
        raise HTTPException(status_code=404, detail="Folder not found")
    try:
        await session.commit()
    except IntegrityError:
        raise HTTPException(status_code=400, detail="A folder with this name already exists in the target location")
    return FolderOut(
        id=folder.id, name=folder.name,
        parent_id=folder.parent_id, sort_order=folder.sort_order,
        color=folder.color, icon=folder.icon,
    )


@router.delete("/folders/{folder_id}", status_code=204)
async def delete_folder(
    folder_id: int,
    session: AsyncSession = Depends(get_session),
):
    repo = FolderRepository(session)
    deleted = await repo.delete(folder_id)
    if not deleted:
        raise HTTPException(status_code=404, detail="Folder not found")
    await session.commit()
